#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Readable.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Threading;

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

        #region DefaultContext

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData,TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <overloads>The overloads of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> method can be grouped into the following categories:
        /// <list type="bullet">
        /// <item>The ones without <see cref="KnownPixelFormat"/> and <see cref="Rectangle"/> parameters attempt to create an exact copy of the original bitmap data.
        /// The original pixel format is attempted to be preserved even for custom formats, which may succeed if the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/>
        /// property was set to <see langword="true"/> when the bitmap data was created.</item>
        /// <item>The overloads that have a <see cref="KnownPixelFormat"/> parameter create a copy with the specified pixel format. The original color depth is attempted to be preserved as much as possible,
        /// even between different pixel formats with wide colors.</item>
        /// <item>The overloads with a <see cref="Rectangle"/> parameter allow to create a copy only of a portion of the source bitmap.</item>
        /// <item>If an overload has an <see cref="IQuantizer"/> parameter, then it allows limiting the set of colors of the result even if the format would allow more colors.</item>
        /// <item>If the result pixel format has a low bit-per-pixel value, or you use a quantizer and you want to preserve the details as much as possible, then look for the
        /// overloads that have an <see cref="IDitherer"/> parameter.</item>
        /// <item>To be able to configure the degree of parallelism, cancellation or progress reporting, look for the overloads whose last parameter is
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm">ParallelConfig</a> instance.</item>
        /// <item>Some overloads have an <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> parameter.
        /// These methods are special ones and are designed to be used from your custom asynchronous methods where cloning is just one step of potentially multiple operations.
        /// But you can also use these overloads to force synchronous execution on a single thread.
        /// See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</item>
        /// <item>All of these methods block the caller on the current thread. For asynchronous call
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CloneAsync">CloneAsync</see> overloads (on .NET Framework 4.0 and above),
        /// or the old-fashioned <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginClone">BeginClone</see> methods that work on every platform target.</item>
        /// </list></overloads>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source)
        {
            ValidateArguments(source);
            return DoCloneExact(AsyncHelper.DefaultContext, source, source.WorkingColorSpace)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, WorkingColorSpace, ParallelConfig)"/> overload to configure these, while still executing the method synchronously.
        /// Alternatively, use the <see cref="BeginClone(IReadableBitmapData, WorkingColorSpace, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, WorkingColorSpace, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, WorkingColorSpace workingColorSpace)
        {
            ValidateArguments(source, workingColorSpace);
            return DoCloneExact(AsyncHelper.DefaultContext, source, workingColorSpace)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, Rectangle, AsyncConfig?)"/> or <see cref="CloneAsync(IReadableBitmapData, Rectangle, TaskConfig?)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle)
        {
            ValidateArguments(source);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, sourceRectangle, source.PixelFormat.AsKnownPixelFormatInternal,
                source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Color32, byte, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_1.htm">ConvertPixelFormat(Image, PixelFormat, Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, WorkingColorSpace, Color32, byte, ParallelConfig)"/> overload to force synchronous execution on a single thread. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_1.htm">ConvertPixelFormat(Image, PixelFormat, Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace,
            Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat, workingColorSpace);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, workingColorSpace, null)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, sourceRectangle, pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, WorkingColorSpace, Color32, byte, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(source, pixelFormat, workingColorSpace);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, sourceRectangle, pixelFormat, backColor, alphaThreshold, workingColorSpace, null)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Palette, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, new Rectangle(Point.Empty, source.Size), pixelFormat,
                palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(AsyncHelper.DefaultContext, source, sourceRectangle, pixelFormat,
                palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually be quantized to <paramref name="pixelFormat"/>, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth can be preserved if <paramref name="quantizer"/> is not specigied and the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncHelper.DefaultContext, source, new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a>extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, IDitherer? ditherer)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncHelper.DefaultContext, source, new Rectangle(Point.Empty, source.Size), pixelFormat, null, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat, IDitherer? ditherer)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncHelper.DefaultContext, source, sourceRectangle, pixelFormat, null, ditherer)!;
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually be quantized to <paramref name="pixelFormat"/>, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth can be preserved if <paramref name="quantizer"/> is not specigied and the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(AsyncHelper.DefaultContext, source, sourceRectangle, pixelFormat, quantizer, ditherer)!;
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads have no default parameters to prevent auto switching to these instead of the original ones
        // because the nullable return value could cause new analyzer issues at the caller side

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData,TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationSynchronously(ctx => DoCloneExact(ctx, source, source.WorkingColorSpace), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginClone(IReadableBitmapData, WorkingColorSpace, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, WorkingColorSpace, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, WorkingColorSpace workingColorSpace, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationSynchronously(ctx => DoCloneExact(ctx, source, workingColorSpace), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/>
        /// parameter allows you to configure the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, Rectangle, AsyncConfig?)"/> or <see cref="CloneAsync(IReadableBitmapData, Rectangle, TaskConfig?)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, Rectangle sourceRectangle, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source, sourceRectangle,
                source.PixelFormat.AsKnownPixelFormatInternal, source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Palette, ParallelConfig)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_1.htm">ConvertPixelFormat(Image, PixelFormat, Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
               new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, Palette, ParallelConfig)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_1.htm">ConvertPixelFormat(Image, PixelFormat, Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace,
            Color32 backColor, byte alphaThreshold, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, workingColorSpace, null), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette, ParallelConfig)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                sourceRectangle, pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <para>If <paramref name="pixelFormat"/> represents an indexed format, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>. To specify the desired palette of the result use the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette, ParallelConfig)"/> overload.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                sourceRectangle, pixelFormat, backColor, alphaThreshold, workingColorSpace, null), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Palette? palette, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source, new Rectangle(Point.Empty, source.Size), pixelFormat,
                palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>This overload automatically quantizes colors if <paramref name="pixelFormat"/> represents a narrower set of colors than <paramref name="source"/>&#160;<see cref="IBitmapData.PixelFormat"/>.
        /// To use a custom quantizer use the overloads with an <see cref="IQuantizer"/> parameter.</para>
        /// <para>Color depth can be preserved if the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat_2.htm">ConvertPixelFormat(Image, PixelFormat, Color[], Color, byte)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            Palette? palette, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source, sourceRectangle, pixelFormat,
                palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size and the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually be quantized to <paramref name="pixelFormat"/>, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth can be preserved if <paramref name="quantizer"/> is not specigied and the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneWithQuantizer(ctx, source, new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer), parallelConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation
        /// was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginClone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, AsyncConfig)"/> or <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer, Rectangle?, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>If <paramref name="pixelFormat"/> can represent a narrower set of colors, then the result will be automatically quantized to its color space,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="pixelFormat"/> can represent,
        /// then the result will eventually be quantized to <paramref name="pixelFormat"/>, though the result may have a poorer quality than expected.</para>
        /// <para>Color depth can be preserved if <paramref name="quantizer"/> is not specigied and the target format can represent the colors of the source format without losing information.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat(Image, PixelFormat, IQuantizer?, IDitherer?)</a> extension method
        /// for some examples. The <a href="https://docs.kgysoft.net/drawing/html/Overload_KGySoft_Drawing_ImageExtensions_ConvertPixelFormat.htm">ConvertPixelFormat</a> extensions work the same way
        /// for <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Image" target="_blank">Image</a>s
        /// as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> extensions for <see cref="IReadableBitmapData"/> instances.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, Rectangle sourceRectangle, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationSynchronously<IReadWriteBitmapData?>(ctx => DoCloneWithQuantizer(ctx, source, sourceRectangle, pixelFormat, quantizer, ditherer), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation. If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, IAsyncContext? context, Rectangle? sourceRectangle = null)
        {
            ValidateArguments(source);
            context ??= AsyncHelper.DefaultContext;
            return sourceRectangle == null
                ? DoCloneExact(context, source, source.WorkingColorSpace)
                : DoCloneDirect(context, source, sourceRectangle.Value, source.PixelFormat.AsKnownPixelFormatInternal, source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation. If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned,
        /// or <see langword="null"/> to clone the whole <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, IAsyncContext? context, KnownPixelFormat pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(context ?? AsyncHelper.DefaultContext, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation. If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned,
        /// or <see langword="null"/> to clone the whole <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, IAsyncContext? context, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null)
        {
            ValidateArguments(source, pixelFormat, workingColorSpace);
            return DoCloneDirect(context ?? AsyncHelper.DefaultContext, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, backColor, alphaThreshold, workingColorSpace, null);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation. If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned,
        /// or <see langword="null"/> to clone the whole <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, IAsyncContext? context, KnownPixelFormat pixelFormat,
            Palette? palette, Rectangle? sourceRectangle = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneDirect(context ?? AsyncHelper.DefaultContext, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/>,
        /// along with a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation. If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned,
        /// or <see langword="null"/> to clone the whole <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, IQuantizer?, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData? Clone(this IReadableBitmapData source, IAsyncContext? context, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer = null, Rectangle? sourceRectangle = null)
        {
            ValidateArguments(source, pixelFormat);
            return DoCloneWithQuantizer(context ?? AsyncHelper.DefaultContext, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to clone the specified <paramref name="source"/> with identical size asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.BeginOperation(ctx => DoCloneExact(ctx, source, source.WorkingColorSpace), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified <paramref name="source"/> with identical size asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, WorkingColorSpace workingColorSpace, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, workingColorSpace);
            return AsyncHelper.BeginOperation(ctx => DoCloneExact(ctx, source, workingColorSpace), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of the specified <paramref name="source"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, Rectangle, TaskConfig?)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, Rectangle sourceRectangle, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle, source.PixelFormat.AsKnownPixelFormatInternal,
                source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
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
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Color32, byte, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat, workingColorSpace);
            return AsyncHelper.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, backColor, alphaThreshold, workingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Palette? palette, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.BeginOperation(ctx => DoCloneDirect(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size),
                pixelFormat, palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette), asyncConfig);
        }

        /// <summary>
        /// Begins to clone the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CloneAsync(IReadableBitmapData, KnownPixelFormat, Palette, Rectangle?, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndClone">EndClone</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static IAsyncResult BeginClone(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer = null, Rectangle? sourceRectangle = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.BeginOperation(ctx => DoCloneWithQuantizer(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginClone">BeginClone</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CloneAsync">CloneAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndClone(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<IReadWriteBitmapData?>(asyncResult, nameof(BeginClone));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync(ctx => DoCloneExact(ctx, source, source.WorkingColorSpace), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified <paramref name="source"/> with identical size asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginClone(IReadableBitmapData, AsyncConfig)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, WorkingColorSpace workingColorSpace, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, workingColorSpace);
            return AsyncHelper.DoOperationAsync(ctx => DoCloneExact(ctx, source, workingColorSpace), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of the specified <paramref name="source"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Alternatively, you can also use the <see cref="BeginClone(IReadableBitmapData, Rectangle, AsyncConfig?)"/> method, which is available on every platform.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, Rectangle sourceRectangle, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source, sourceRectangle,
                source.PixelFormat.AsKnownPixelFormatInternal, source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="workingColorSpace">Specifies the value of the <see cref="IBitmapData.WorkingColorSpace"/> property of the cloned instance
        /// and affects the possible blending operations during the cloning.</param>
        /// <param name="backColor">If <paramref name="pixelFormat"/> does not support alpha or supports only single-bit alpha, then specifies the color of the background.
        /// Source pixels with alpha, which will be opaque in the result will be blended with this color.
        /// The <see cref="Color32.A">Color32.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If <paramref name="pixelFormat"/> can represent only single-bit alpha or <paramref name="pixelFormat"/> is an indexed format and the target palette contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the result will not have transparent pixels. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat, workingColorSpace);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, workingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and color settings asynchronously.
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
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Color32, byte)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat, backColor, alphaThreshold, source.WorkingColorSpace, null), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/> and <paramref name="palette"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> is an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// If <see langword="null"/>, then the target palette is taken from <paramref name="source"/> if it also has a palette of no more entries than the target indexed format can have;
        /// otherwise, a default palette will be used based on <paramref name="pixelFormat"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, Palette)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            Palette? palette, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneDirect(ctx, source,
                sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat,
                palette?.BackColor ?? source.BackColor, palette?.AlphaThreshold ?? source.AlphaThreshold, palette?.WorkingColorSpace ?? source.WorkingColorSpace, palette), asyncConfig);
        }

        /// <summary>
        /// Gets the clone of the specified portion of <paramref name="source"/> with the specified <paramref name="pixelFormat"/>, using an optional <paramref name="quantizer"/> and <paramref name="ditherer"/> asynchronously.
        /// </summary>
        /// <param name="source">An <see cref="IReadableBitmapData"/> instance to be cloned.</param>
        /// <param name="pixelFormat">The desired pixel format of the result.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/> and <paramref name="pixelFormat"/> is an indexed format, then a default palette and quantization logic will be used.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="pixelFormat"/> represents an at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be cloned, or <see langword="null"/> to clone the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the new <see cref="IReadWriteBitmapData"/> instance that represents the clone of the specified <paramref name="source"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Clone(IReadableBitmapData, Rectangle, KnownPixelFormat, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> does not specify a valid format.
        /// <br/>-or-
        /// <br/><paramref name="sourceRectangle"/> has no overlapping region with source bounds.</exception>
        /// <exception cref="ArgumentException"><paramref name="quantizer"/> uses a palette with too many colors for the specified <paramref name="pixelFormat"/>.</exception>
        public static Task<IReadWriteBitmapData?> CloneAsync(this IReadableBitmapData source, KnownPixelFormat pixelFormat,
            IQuantizer? quantizer, IDitherer? ditherer = null, Rectangle? sourceRectangle = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, pixelFormat);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneWithQuantizer(ctx, source, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region CopyTo

        #region Sync

        #region DefaultContext

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <overloads>The overloads of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> method can be grouped into the following categories:
        /// <list type="bullet">
        /// <item>The overloads with a <see cref="Rectangle"/> parameter allow to copy only a portion of the source bitmap.</item>
        /// <item>If an overload has an <see cref="IQuantizer"/> parameter, then it allows limiting the set of colors of the result even if the pixel format of the target would allow more colors.</item>
        /// <item>If the target pixel format has a low bit-per-pixel value or you use a quantizer and you want to preserve the details as much as possible, then look for the
        /// overloads that have an <see cref="IDitherer"/> parameter.</item>
        /// <item>To be able to configure the degree of parallelism, cancellation or progress reporting, look for the overloads whose last parameter is
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm">ParallelConfig</a> instance.</item>
        /// <item>One overload has an <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> parameter.
        /// That method is a special one and is designed to be used from your custom asynchronous methods where copying a bitmap is just one step of potentially multiple operations.
        /// But you can also use that overload to force synchronous execution on a single thread.
        /// See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</item>
        /// <item>All of these methods block the caller on the current thread. For asynchronous call
        /// you can use the <see cref="CopyToAsync">CopyToAsync</see> method (on .NET Framework 4.0 and above),
        /// or the old-fashioned <see cref="BeginCopyTo">BeginCopyTo</see> method that works on every platform target.</item>
        /// </list>
        /// <note>Note that these methods preserve the original size of the source bitmap, and copy even the alpha pixels without alpha blending.
        /// To draw a bitmap data into another one with blending and potential resizing, use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.</note>
        /// </overloads>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IDitherer? ditherer)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IDitherer? ditherer)
            => CopyTo(source, target, sourceRectangle, targetLocation, null, ditherer);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            => source.CopyTo(target, AsyncHelper.DefaultContext, sourceRectangle, targetLocation, quantizer, ditherer);

        #endregion

        #region ParallelConfig/IAsyncContext
        // NOTE: The overloads with ParallelConfig have no default parameters to prevent auto switching to these instead of the original ones.
        // Even though it would be compile-compatible, these overloads have bool return value, and there is a minimal overhead with the DoOperationSynchronously call.

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. </param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => CopyTo(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, quantizer, ditherer, parallelConfig);

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. </param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism,
        /// cancellation and progress reporting. Use the <see cref="BeginCopyTo">BeginCopyTo</see> or <see cref="CopyToAsync">CopyToAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The copied area is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool CopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, target);
            return AsyncHelper.DoOperationSynchronously(ctx => DoCopy(ctx, source, target, sourceRectangle, targetLocation, quantizer, ditherer), parallelConfig);
        }

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/>
        /// without scaling and blending, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer?, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool CopyTo(this IReadableBitmapData source, IWritableBitmapData target, IAsyncContext? context, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, target);
            return DoCopy(context ?? AsyncHelper.DefaultContext, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        } 

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to copy the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/> asynchronously,
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginDrawInto">BeginDrawInto</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied, or <see langword="null"/> to copy the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/> top copy the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="CopyToAsync">CopyToAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndCopyTo">EndCopyTo</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginCopyTo(this IReadableBitmapData source, IWritableBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            return AsyncHelper.BeginOperation(ctx => DoCopy(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginCopyTo">BeginCopyTo</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndCopyTo(this IAsyncResult asyncResult)
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginCopyTo));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Copies the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IWritableBitmapData"/> asynchronously,
        /// without scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To draw a bitmap data into another one with blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawIntoAsync">DrawIntoAsync</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be copied into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IWritableBitmapData"/> into which <paramref name="source"/> should be copied.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be copied, or <see langword="null"/> to copy the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/> top copy the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used. If not specified, then the copying operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="CopyTo(IReadableBitmapData, IWritableBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static Task CopyToAsync(this IReadableBitmapData source, IWritableBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            ValidateArguments(source, target);
            return AsyncHelper.DoOperationAsync(ctx => DoCopy(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region DrawInto

        #region Without resize

        #region Sync

        #region DefaultContext

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/> or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <overloads>The overloads of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> method can be grouped into the following categories:
        /// <list type="bullet">
        /// <item>The ones that have a <see cref="Point"/> parameter for target location preserve the original size of the source bitmap.</item>
        /// <item>The overloads with a <see cref="Rectangle"/> and <see cref="Point"/> parameter allow to draw only a portion of the source bitmap, while they still preserve the original size.</item>
        /// <item>There are overloads that allow resizing. These either have no <see cref="Point"/> parameter but one <see cref="Rectangle"/> argument to allow drawing the whole source bitmap into the specified target rectangle;
        /// or, they have two <see cref="Rectangle"/> parameters to allow drawing a portion of the source bitmap into the specified target rectangle. All of these methods have also
        /// a <see cref="ScalingMode"/> parameter that specifies the behavior of the potential shrinking or enlarging.</item>
        /// <item>If an overload has an <see cref="IQuantizer"/> parameter, then it allows limiting the set of colors of the result even if the pixel format of the target would allow more colors.</item>
        /// <item>If the target pixel format has a low bit-per-pixel value or you use a quantizer and you want to preserve the details as much as possible, then look for the
        /// overloads that have an <see cref="IDitherer"/> parameter.</item>
        /// <item>To be able to configure the degree of parallelism, cancellation or progress reporting, look for the overloads whose last parameter is
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm">ParallelConfig</a> instance.</item> 
        /// <item>A couple of overloads have an <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> parameter.
        /// These methods are special ones and are designed to be used from your custom asynchronous methods where drawing a bitmap into another one is just one step of potentially multiple operations.
        /// But you can also use these overloads to force synchronous execution on a single thread.
        /// See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</item>
        /// <item>All of these methods block the caller on the current thread. For asynchronous call
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawIntoAsync">DrawIntoAsync</see> overloads (on .NET Framework 4.0 and above),
        /// or the old-fashioned <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginDrawInto">BeginDrawInto</see> methods that work on every platform target.</item>
        /// </list>
        /// <note>Note that these methods always perform an alpha blending (respecting the <see cref="IBitmapData.WorkingColorSpace"/> of the target bitmap) if the source contains alpha pixels.
        /// To copy a bitmap data into another one without blending and resizing, use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.</note>
        /// </overloads>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, quantizer, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>.
        /// To use dithering a <paramref name="ditherer"/> must be explicitly specified.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IDitherer? ditherer)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, null, ditherer);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
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
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            => source.DrawInto(target, AsyncHelper.DefaultContext, sourceRectangle, targetLocation, quantizer, ditherer);

        #endregion

        #region ParallelConfig/IAsyncContext
        // NOTE: The overloads with ParallelConfig have no default parameters to prevent auto switching to these instead of the original ones.
        // Even though it would be compile-compatible, these overloads have bool return value, and there is a minimal overhead with the DoOperationSynchronously call.

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/> or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetLocation, quantizer, ditherer, parallelConfig);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the overloads with <c>targetRectangle</c> and <see cref="ScalingMode"/> parameters.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, AsyncConfig)"/> or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The image to be drawn is automatically clipped if its size or <paramref name="targetLocation"/> makes it impossible to completely fit in the <paramref name="target"/>.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods.</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, target);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawInto(ctx, source, target, sourceRectangle, targetLocation, quantizer, ditherer), parallelConfig);
        }

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// without scaling, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetLocation">The target location. Target size will be always the same as the source size.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer?, IDitherer?)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, IAsyncContext? context, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            ValidateArguments(source, target);
            return DoDrawInto(context ?? AsyncHelper.DefaultContext, source, target, sourceRectangle, targetLocation, quantizer, ditherer);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/> overload.
        /// To copy a bitmap data into another one without blending use the <see cref="BeginCopyTo">BeginCopyTo</see> method instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>, or <see langword="null"/> to draw the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/> top draw the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle?, Point?, IQuantizer, IDitherer, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndDrawInto">EndDrawInto</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static IAsyncResult BeginDrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target);
            return AsyncHelper.BeginOperation(ctx => DoDrawInto(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginDrawInto">BeginDrawInto</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawIntoAsync">DrawIntoAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndDrawInto(this IAsyncResult asyncResult) 
            // NOTE: the return value could be bool, but it would be a breaking change
            => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawInto));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// without scaling, using blending. This method always preserves the source size in pixels, works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// For scaling use the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> overload.
        /// To copy a bitmap data into another one without blending use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>, or <see langword="null"/> to draw the entire <paramref name="source"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="targetLocation">A <see cref="Point"/> that specifies the target location, or <see langword="null"/> top draw the <paramref name="source"/> to the top-left corner of the <paramref name="target"/>. Target size will be always the same as the source size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to be used for the drawing. If not specified, then the drawing operation might automatically
        /// pick a quantizer based on <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/>. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="quantizer"/> is not specified
        /// and <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Point, IQuantizer, IDitherer)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        public static Task DrawIntoAsync(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle? sourceRectangle = null, Point? targetLocation = null, IQuantizer? quantizer = null, IDitherer? ditherer = null, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            ValidateArguments(source, target);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawInto(ctx, source, target, sourceRectangle ?? new Rectangle(Point.Empty, source.Size), targetLocation ?? Point.Empty, quantizer, ditherer), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region With resize

        #region Sync

        #region DefaultContext

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetRectangle, quantizer, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="ditherer">The ditherer to be used for the drawing. Might be ignored if <paramref name="target"/>&#160;<see cref="IBitmapData.PixelFormat"/> format has at least 24 bits-per-pixel size.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
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
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetRectangle, null, ditherer, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
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
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetRectangle, null, null, scalingMode);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="sourceRectangle">A <see cref="Rectangle"/> that specifies the portion of the <paramref name="source"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="targetRectangle">A <see cref="Rectangle"/> that specifies the location and size of the drawn <paramref name="source"/>.</param>
        /// <param name="scalingMode">Specifies the scaling mode if the bitmap data to be drawn needs to be resized.</param>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
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
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
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
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use
        /// the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, ParallelConfig)"/> overload to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The method has the best performance if <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="sourceRectangle"/> or <paramref name="targetRectangle"/> exceed bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="sourceRectangle"/> and <paramref name="targetRectangle"/> are different.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static void DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
            => source.DrawInto(target, AsyncHelper.DefaultContext, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);

        #endregion

        #region ParallelConfig/IAsyncContext
        // NOTE: The overloads with ParallelConfig have no default parameters to prevent auto switching to these instead of the original ones.
        // Even though it would be compile-compatible, these overloads have bool return value, and there is a minimal overhead with the DoOperationSynchronously call.

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode, ParallelConfig? parallelConfig)
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - needed to avoid NullReferenceException if source is null so ArgumentNullException is thrown from ValidateArguments
            => DrawInto(source, target, new Rectangle(Point.Empty, source?.Size ?? default), targetRectangle, quantizer, ditherer, scalingMode, parallelConfig);

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods instead.
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
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="BeginDrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, AsyncConfig)"/>
        /// or <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// <para>The method has the best performance if <paramref name="source"/> and <paramref name="targetRectangle"/> have the same size, or when <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>.</para>
        /// <para>The image to be drawn is automatically clipped if <paramref name="targetRectangle"/> exceeds bounds, or <paramref name="scalingMode"/> is <see cref="ScalingMode.NoScaling"/>
        /// and <paramref name="source"/> and <paramref name="targetRectangle"/> have different sizes.</para>
        /// <para><paramref name="target"/> must be an <see cref="IReadWriteBitmapData"/> because it must be readable if blending is necessary. For write-only <see cref="IWritableBitmapData"/> instances
        /// you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.CopyTo">CopyTo</see> methods (without scaling).</para>
        /// <para>If <paramref name="target"/> can represent a narrower set of colors, then the result will be automatically quantized to the colors of the <paramref name="target"/>,
        /// even if there is no <paramref name="quantizer"/> specified. To use dithering a <paramref name="ditherer"/> must be explicitly specified though.</para>
        /// <para>If <paramref name="quantizer"/> is specified but it uses more/different colors than <paramref name="target"/> can represent,
        /// then the result will eventually be quantized to <paramref name="target"/>, though the result may have a poorer quality than expected.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode, ParallelConfig? parallelConfig)
        {
            ValidateArguments(source, target, scalingMode);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawInto(ctx, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode), parallelConfig);
        }

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/>
        /// with scaling and blending, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to be drawn into the <paramref name="target"/>.</param>
        /// <param name="target">The target <see cref="IReadWriteBitmapData"/> into which <paramref name="source"/> should be drawn.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
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
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// <note>See the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer?, IDitherer?, ScalingMode)"/> overload for more details about the other parameters.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static bool DrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, IAsyncContext? context, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto)
        {
            ValidateArguments(source, target, scalingMode);
            return DoDrawInto(context ?? AsyncHelper.DefaultContext, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to draw the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="BeginCopyTo">BeginCopyTo</see> method instead.
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
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="DrawIntoAsync(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode, TaskConfig)"/> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndDrawInto">EndDrawInto</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static IAsyncResult BeginDrawInto(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, target, scalingMode);
            return AsyncHelper.BeginOperation(ctx => DoDrawInto(ctx, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode), asyncConfig);
        }

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Draws the <paramref name="source"/>&#160;<see cref="IReadableBitmapData"/> into the <paramref name="target"/>&#160;<see cref="IReadWriteBitmapData"/> asynchronously,
        /// using scaling and blending. This method works between any pair of source and target <see cref="KnownPixelFormat"/>s and supports quantizing and dithering.
        /// To copy a bitmap data into another one without blending use the <see cref="CopyToAsync">CopyToAsync</see> method instead.
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
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="DrawInto(IReadableBitmapData, IReadWriteBitmapData, Rectangle, Rectangle, IQuantizer, IDitherer, ScalingMode)"/> method for more details.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static Task DrawIntoAsync(this IReadableBitmapData source, IReadWriteBitmapData target, Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer = null, IDitherer? ditherer = null, ScalingMode scalingMode = ScalingMode.Auto, TaskConfig? asyncConfig = null)
        {
            // NOTE: the return value could be Task<bool> but it would be a breaking change
            ValidateArguments(source, target, scalingMode);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawInto(ctx, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode), asyncConfig);
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
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <param name="disposeSource"><see langword="true"/> to dispose <paramref name="source"/> when the result is disposed; otherwise, <see langword="false"/>.</param>
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
            ValidateArguments(source);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.Size
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

        #region GetColors

        /// <summary>
        /// Gets the colors used in the specified <paramref name="bitmapData"/>. A limit can be defined in <paramref name="maxColors"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/> to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="Color32"/> entries.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginGetColors">BeginGetColors</see>
        /// or <see cref="GetColorsAsync">GetColorsAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="KnownPixelFormat"/> is supported, though wide color formats (<see cref="KnownPixelFormat.Format16bppGrayScale"/>, <see cref="KnownPixelFormat.Format48bppRgb"/>,
        /// <see cref="KnownPixelFormat.Format64bppArgb"/> and <see cref="KnownPixelFormat.Format64bppPArgb"/>) are quantized to 32 bit during the processing.
        /// To get the actual <em>number</em> of colors, which can be accurate even for wide color formats, use the <see cref="GetColorCount">GetColorCount</see> method.
        /// </para>
        /// </remarks>
        public static ICollection<Color32> GetColors(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            if (!forceScanningContent && bitmapData.PixelFormat.Indexed && bitmapData.Palette != null)
                return bitmapData.Palette.GetEntries();

            return DoGetColors<Color32>(AsyncHelper.DefaultContext, bitmapData, maxColors);
        }

        /// <summary>
        /// Begins to get the colors used in the specified <paramref name="bitmapData"/> asynchronously. A limit can be defined in <paramref name="maxColors"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/> to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="GetColorsAsync">GetColorsAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndGetColors">EndGetColors</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColors">GetColors</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginGetColors(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            return !forceScanningContent && bitmapData.PixelFormat.Indexed && bitmapData.Palette != null
                ? AsyncHelper.FromResult(bitmapData.Palette.GetEntries(), Reflector.EmptyArray<Color32>(), asyncConfig)
                : AsyncHelper.BeginOperation(ctx => DoGetColors<Color32>(ctx, bitmapData, maxColors), Reflector.EmptyArray<Color32>(), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginGetColors">BeginGetColors</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="GetColorsAsync">GetColorsAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="Color32"/> entries that is the result of the operation.
        /// If the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>, then an empty collection is returned.</returns>
        public static ICollection<Color32> EndGetColors(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<ICollection<Color32>>(asyncResult, nameof(BeginGetColors));

#if !NET35
        /// <summary>
        /// Gets the colors used in the specified <paramref name="bitmapData"/> asynchronously. A limit can be defined in <paramref name="maxColors"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/>, whose colors have to be returned. If it is indexed and the <paramref name="forceScanningContent"/> parameter is <see langword="false"/>,
        /// then its palette entries are returned and <paramref name="maxColors"/> is ignored.</param>
        /// <param name="maxColors">A limit of the returned colors. If <paramref name="forceScanningContent"/> parameter is <see langword="false"/>, then
        /// this parameter is ignored for indexed bitmaps. Use 0 for no limit. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="forceScanningContent"><see langword="true"/> to force scanning the actual image content even if the specified <paramref name="bitmapData"/> is
        /// indexed and has a palette. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="ICollection{T}"/> of <see cref="Color32"/> entries.
        /// If the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>, then the result of the task is an empty collection.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColors">GetColors</see> method for more details.</note>
        /// </remarks>
        public static Task<ICollection<Color32>> GetColorsAsync(this IReadableBitmapData bitmapData, int maxColors = 0, bool forceScanningContent = false, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);

            return !forceScanningContent && bitmapData.PixelFormat.Indexed && bitmapData.Palette != null
                ? AsyncHelper.FromResult((ICollection<Color32>)bitmapData.Palette.GetEntries(), Reflector.EmptyArray<Color32>(), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoGetColors<Color32>(ctx, bitmapData, maxColors), Reflector.EmptyArray<Color32>(), asyncConfig);
        }
#endif

        #endregion

        #region GetColorCount

        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/>. Colors are counted even for indexed bitmaps.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <returns>The actual number of colors of the specified <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginGetColorCount">BeginGetColorCount</see>
        /// or <see cref="GetColorCountAsync">GetColorCountAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>Completely transparent pixels are considered the same regardless of their color information.</para>
        /// <para>Every <see cref="KnownPixelFormat"/> is supported, and an accurate result can be retrieved even for custom pixel formats with wide color formats
        /// if color access preference id correctly set in their <see cref="PixelFormatInfo"/>.
        /// Otherwise, colors might be quantized to 32 bits-per-pixel values while counting them.</para>
        /// </remarks>
        public static int GetColorCount(this IReadableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoGetColorCount(AsyncHelper.DefaultContext, bitmapData);
        }

        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/> asynchronously. Colors are counted even for indexed bitmaps.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="GetColorCountAsync">GetColorCountAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndGetColorCount">EndGetColorCount</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColorCount">GetColorCount</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginGetColorCount(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoGetColorCount(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginGetColorCount">BeginGetColorCount</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="GetColorCountAsync">GetColorCountAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="int">int</see> value that is the result of the operation,
        /// or <c>0</c>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static int EndGetColorCount(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<int>(asyncResult, nameof(BeginGetColorCount));

#if !NET35
        /// <summary>
        /// Gets the actual number of colors of the specified <paramref name="bitmapData"/> asynchronously. Colors are counted even for indexed bitmaps.
        /// </summary>
        /// <param name="bitmapData">The bitmap, whose colors have to be counted to count its colors.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is the actual number of colors of the specified <paramref name="bitmapData"/>,
        /// or <c>0</c>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="GetColorCount">GetColorCount</see> method for more details.</note>
        /// </remarks>
        public static Task<int> GetColorCountAsync(this IReadableBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync(ctx => DoGetColorCount(ctx, bitmapData), asyncConfig);
        }
#endif

        #endregion

        #region ToGrayscale

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the grayscale version of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToGrayscale">BeginToGrayscale</see>
        /// or <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="KnownPixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To return an <see cref="IReadWriteBitmapData"/> with arbitrary <see cref="IBitmapData.PixelFormat"/> use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> overloads with a grayscale palette,
        /// quantizer (e.g. <see cref="PredefinedColorsQuantizer.Grayscale(Color32,byte)">PredefinedColorsQuantizer.Grayscale</see>) or pixel format (<see cref="KnownPixelFormat.Format16bppGrayScale"/>).</para>
        /// <para>To make an <see cref="IReadWriteBitmapData"/> grayscale without creating a new instance use the <see cref="MakeGrayscale(IReadWriteBitmapData,IDitherer?)">MakeGrayscale</see> method.</para>
        /// </remarks>
        /// <seealso cref="MakeGrayscale(IReadWriteBitmapData,IDitherer?)"/>
        public static IReadWriteBitmapData ToGrayscale(this IReadableBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return DoCloneWithQuantizer(AsyncHelper.DefaultContext, bitmapData, new Rectangle(Point.Empty, bitmapData.Size), KnownPixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray()))!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to grayscale asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToGrayscale">EndToGrayscale</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToGrayscale">ToGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeGrayscale"/>
        public static IAsyncResult BeginToGrayscale(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoCloneWithQuantizer(ctx, bitmapData, new Rectangle(Point.Empty, bitmapData.Size), KnownPixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray())), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginToGrayscale">BeginToGrayscale</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ToGrayscaleAsync">ToGrayscaleAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndToGrayscale(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<IReadWriteBitmapData?>(asyncResult, nameof(BeginToGrayscale));

#if !NET35
        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the grayscale version of the specified <paramref name="bitmapData"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to grayscale.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/> containing the grayscale version of the original <paramref name="bitmapData"/>,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToGrayscale">ToGrayscale</see> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeGrayscaleAsync"/>
        public static Task<IReadWriteBitmapData?> ToGrayscaleAsync(this IReadWriteBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync<IReadWriteBitmapData?>(ctx => DoCloneWithQuantizer(ctx, bitmapData, new Rectangle(Point.Empty, bitmapData.Size), KnownPixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray())), asyncConfig);
        }
#endif

        #endregion

        #region ToTransparent

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <returns>A new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToTransparent(IReadableBitmapData, AsyncConfig)"/>
        /// or <see cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method uses the bottom-left pixel to determine the background color, which must be completely opaque; otherwise, just an exact clone of <paramref name="bitmapData"/> will be returned.</para>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="KnownPixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To attempt to make an <see cref="IReadWriteBitmapData"/> transparent without creating a new instance use the <see cref="MakeTransparent(IReadWriteBitmapData)">MakeTransparent</see> method.</para>
        /// <para>To force replacing even non-completely opaque pixels use the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> method with a quantizer
        /// created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32, Color32}, KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparent(IReadWriteBitmapData)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)"/>
        public static IReadWriteBitmapData ToTransparent(this IReadableBitmapData bitmapData)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return DoToTransparent(AsyncHelper.DefaultContext, bitmapData)!;
        }

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <returns>A new <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginToTransparent(IReadableBitmapData, Color32, AsyncConfig)"/>
        /// or <see cref="ToTransparentAsync(IReadableBitmapData, Color32, TaskConfig)"/> (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>This method always returns a new <see cref="IReadWriteBitmapData"/> with <see cref="KnownPixelFormat.Format32bppArgb"/> pixel format.</para>
        /// <para>To attempt to make an <see cref="IReadWriteBitmapData"/> transparent without creating a new instance use the <see cref="MakeTransparent(IReadWriteBitmapData,Color32)">MakeTransparent</see> method.</para>
        /// <para>To auto-detect the background color to be made transparent use the <see cref="ToTransparent(IReadableBitmapData)"/> overload instead.</para>
        /// <note>Please note that unlike the <see cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)">MakeOpaque</see> method, this one changes exactly one color shade without any tolerance.
        /// For any customization use the <see cref="Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer, IDitherer)">Clone</see> method with a quantizer
        /// created by the <see cref="PredefinedColorsQuantizer.FromCustomFunction(Func{Color32, Color32}, KnownPixelFormat)">PredefinedColorsQuantizer.FromCustomFunction</see> method.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparent(IReadWriteBitmapData,Color32)"/>
        /// <seealso cref="MakeOpaque(IReadWriteBitmapData,Color32,IDitherer?)"/>
        public static IReadWriteBitmapData ToTransparent(this IReadableBitmapData bitmapData, Color32 transparentColor)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return DoToTransparent(AsyncHelper.DefaultContext, bitmapData, transparentColor)!;
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to another one with transparent background asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToTransparentAsync(IReadableBitmapData, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToTransparent">EndToTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeTransparent(IReadWriteBitmapData, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginToTransparent(this IReadableBitmapData bitmapData, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return AsyncHelper.BeginOperation(ctx => DoToTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Begins to convert the specified <paramref name="bitmapData"/> to another one with transparent background asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ToTransparentAsync(IReadableBitmapData, Color32, TaskConfig)"/> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndToTransparent">EndToTransparent</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="BeginMakeTransparent(IReadWriteBitmapData, Color32, AsyncConfig)"/>
        /// <seealso cref="BeginMakeOpaque"/>
        public static IAsyncResult BeginToTransparent(this IReadableBitmapData bitmapData, Color32 transparentColor, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return AsyncHelper.BeginOperation(ctx => DoToTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.BeginToTransparent">BeginToTransparent</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.ToTransparentAsync">ToTransparentAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndToTransparent(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<IReadWriteBitmapData?>(asyncResult, nameof(BeginToTransparent));

#if !NET35
        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparentAsync(IReadWriteBitmapData, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<IReadWriteBitmapData?> ToTransparentAsync(this IReadableBitmapData bitmapData, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return AsyncHelper.DoOperationAsync(ctx => DoToTransparent(ctx, bitmapData), asyncConfig);
        }

        /// <summary>
        /// Returns a new <see cref="IReadWriteBitmapData"/> asynchronously, which is the clone of the specified <paramref name="bitmapData"/> with transparent background.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to convert to transparent.</param>
        /// <param name="transparentColor">Specifies the color to make transparent.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IReadWriteBitmapData"/>, which is the clone of the specified <paramref name="bitmapData"/> with transparent background,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ToTransparent(IReadableBitmapData, Color32)"/> method for more details.</note>
        /// </remarks>
        /// <seealso cref="MakeTransparentAsync(IReadWriteBitmapData, Color32, TaskConfig)"/>
        /// <seealso cref="MakeOpaqueAsync"/>
        public static Task<IReadWriteBitmapData?> ToTransparentAsync(this IReadableBitmapData bitmapData, Color32 transparentColor, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, nameof(bitmapData));
            return AsyncHelper.DoOperationAsync(ctx => DoToTransparent(ctx, bitmapData, transparentColor), asyncConfig);
        }
#endif

        #endregion

        #region Resize

        /// <summary>
        /// Resizes the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to resize.</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="scalingMode">A <see cref="ScalingMode"/> value, which determines the quality of the result as well as the processing time. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="keepAspectRatio"><see langword="true"/> to keep aspect ratio of the specified <paramref name="source"/>; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A new <see cref="IReadWriteBitmapData"/>, which is the resized version of the specified <paramref name="source"/>.</returns>
        /// <remarks>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress.
        /// Use the <see cref="BeginResize">BeginResize</see> or <see cref="ResizeAsync">ResizeAsync</see>
        /// (in .NET Framework 4.0 and above) methods for asynchronous call and to adjust parallelization, set up cancellation and for reporting progress.</note>
        /// <para>The result <see cref="IBitmapData.PixelFormat"/> depends on the <see cref="IBitmapData.WorkingColorSpace"/> of the <paramref name="source"/>
        /// bitmap data but is always at least a 32 BPP format. To resize a bitmap data with a custom pixel format you can create a
        /// new <see cref="IReadWriteBitmapData"/> instance by the <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Color32,byte)"/> method
        /// and use the <see cref="O:KGySoft.Drawing.ImageExtensions.DrawInto">DrawInto</see> extension methods, which has several overloads that also allow quantizing and dithering.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scalingMode"/> has an unsupported value.</exception>
        public static IReadWriteBitmapData Resize(this IReadableBitmapData source, Size newSize, ScalingMode scalingMode = ScalingMode.Auto, bool keepAspectRatio = false)
        {
            ValidateArguments(source, newSize, scalingMode);
            return DoResize(AsyncHelper.DefaultContext, source, newSize, scalingMode, keepAspectRatio)!;
        }

        /// <summary>
        /// Begins to resize the specified <paramref name="source"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to resize.</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="scalingMode">A <see cref="ScalingMode"/> value, which determines the quality of the result as well as the processing time. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="keepAspectRatio"><see langword="true"/> to keep aspect ratio of the specified <paramref name="source"/>; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="ResizeAsync">ResizeAsync</see> method.</para>
        /// <para>To get the result or the exception that occurred during the operation you have to call the <see cref="EndResize">EndResize</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Resize">Resize</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginResize(this IReadableBitmapData source, Size newSize, ScalingMode scalingMode = ScalingMode.Auto, bool keepAspectRatio = false, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(source, newSize, scalingMode);
            return AsyncHelper.BeginOperation(ctx => DoResize(ctx, source, newSize, scalingMode, keepAspectRatio), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginResize">BeginResize</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="ResizeAsync">ResizeAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndResize(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<IReadWriteBitmapData?>(asyncResult, nameof(BeginResize));

#if !NET35
        /// <summary>
        /// Resizes the specified <paramref name="source"/> asynchronously.
        /// </summary>
        /// <param name="source">The source <see cref="IReadableBitmapData"/> to resize.</param>
        /// <param name="newSize">The requested new size.</param>
        /// <param name="scalingMode">A <see cref="ScalingMode"/> value, which determines the quality of the result as well as the processing time. This parameter is optional.
        /// <br/>Default value: <see cref="ScalingMode.Auto"/>.</param>
        /// <param name="keepAspectRatio"><see langword="true"/> to keep aspect ratio of the specified <paramref name="source"/>; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Resize">Resize</see> method for more details.</note>
        /// </remarks>
        public static Task<IReadWriteBitmapData?> ResizeAsync(this IReadableBitmapData source, Size newSize, ScalingMode scalingMode = ScalingMode.Auto, bool keepAspectRatio = false, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(source, newSize, scalingMode);
            return AsyncHelper.DoOperationAsync(ctx => DoResize(ctx, source, newSize, scalingMode, keepAspectRatio), asyncConfig);
        }
#endif

        #endregion

        #region Save

        /// <summary>
        /// Saves the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginSave">BeginSave</see>
        /// or <see cref="SaveAsync">SaveAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// <para>To reload the content use the <see cref="BitmapDataFactory.Load">BitmapDataFactory.Load</see> method.</para>
        /// <para>The saved content always preserves known <see cref="KnownPixelFormat"/>s so the <see cref="BitmapDataFactory.Load">BitmapDataFactory.Load</see>
        /// method can restore it the same way on any platform. Custom pixel formats are saved by a compatible known pixel format.</para>
        /// </remarks>
        public static void Save(this IReadableBitmapData bitmapData, Stream stream)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            DoSave(AsyncHelper.DefaultContext, bitmapData, stream);
        }

        /// <summary>
        /// Begins to save the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="SaveAsync">SaveAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndSave">EndSave</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Save">Save</see> method for more details.</note>
        /// </remarks>
        public static IAsyncResult BeginSave(this IReadableBitmapData bitmapData, Stream stream, AsyncConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoSave(ctx, bitmapData, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginSave">BeginSave</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="SaveAsync">SaveAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public static void EndSave(this IAsyncResult asyncResult) => AsyncHelper.EndOperation(asyncResult, nameof(BeginSave));

#if !NET35
        /// <summary>
        /// Saves the content of this <paramref name="bitmapData"/> into the specified <paramref name="stream"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadableBitmapData"/> to save.</param>
        /// <param name="stream">The stream to save the bitmap data into.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Save">Save</see> method for more details.</note>
        /// </remarks>
        public static Task SaveAsync(this IReadableBitmapData bitmapData, Stream stream, TaskConfig? asyncConfig = null)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync(ctx => DoSave(ctx, bitmapData, stream), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Internal Methods

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context, WorkingColorSpace workingColorSpace) => DoCloneExact(context, source, workingColorSpace);

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context, KnownPixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
            => DoCloneWithQuantizer(context, source, new Rectangle(Point.Empty, source.Size), pixelFormat, quantizer, ditherer);

        internal static IReadWriteBitmapData? DoClone(this IReadableBitmapData source, IAsyncContext context, Rectangle sourceRectangle, KnownPixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer)
            => DoCloneWithQuantizer(context, source, sourceRectangle, pixelFormat, quantizer, ditherer);

        internal static IReadWriteBitmapData DoClone(this IReadableBitmapData source, IAsyncContext context, Rectangle sourceRectangle, KnownPixelFormat pixelFormat, Palette palette)
            => DoCloneDirect(context, source, sourceRectangle, pixelFormat, palette.BackColor, palette.AlphaThreshold, palette.WorkingColorSpace, palette)!;

        internal static void DoCopyTo(this IReadableBitmapData source, IAsyncContext context, IWritableBitmapData target, Point targetLocation = default, IQuantizer? quantizer = null, IDitherer? ditherer = null)
            => DoCopy(context, source, target, new Rectangle(Point.Empty, source.Size), targetLocation, quantizer, ditherer);

        internal static void DoCopyTo(this IReadableBitmapData source, IAsyncContext context, IWritableBitmapData target, Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer = null, bool skipTransparent = false)
            => DoCopy(context, source, target, sourceRectangle, targetLocation, quantizer, null, skipTransparent);

        internal static void DoCopyTo(this IReadableBitmapData source, IAsyncContext context, IWritableBitmapData target, Point targetLocation, IQuantizer quantizer, IDitherer? ditherer, bool skipTransparent, in Array2D<byte> mask, Point maskOffset)
            => DoCopy(context, source, target, new Rectangle(Point.Empty, source.Size), targetLocation, quantizer, ditherer, skipTransparent, mask, maskOffset);

        internal static void DoDrawInto(this IReadableBitmapData source, IAsyncContext context, IReadWriteBitmapData target, Rectangle targetRectangle)
            => DoDrawInto(context, source, target, new Rectangle(Point.Empty, source.Size), targetRectangle, null, null, ScalingMode.Auto);

        internal static IReadWriteBitmapData? DoResize(this IReadableBitmapData bitmapData, IAsyncContext context, Size newSize, ScalingMode scalingMode, bool keepAspectRatio)
            => DoResize(context, bitmapData, newSize, scalingMode, keepAspectRatio);

        #endregion

        #region Private Methods

        #region Validation
        // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local - all of these methods are validations

        private static void ValidateArguments(IReadableBitmapData source, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
            if (workingColorSpace < WorkingColorSpace.Default || workingColorSpace > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
        }

        private static void ValidateArguments(IReadableBitmapData source, string paramName)
        {
            if (source == null)
                throw new ArgumentNullException(paramName, PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, paramName);
        }

        private static void ValidateArguments(IReadableBitmapData source, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            if (source.Width <= 0 || source.Height <= 0)
                throw new ArgumentException(Res.ImagingInvalidBitmapDataSize, nameof(source));
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (workingColorSpace < WorkingColorSpace.Default || workingColorSpace > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
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

        private static void ValidateArguments(IReadableBitmapData bitmapData, Size newSize, ScalingMode scalingMode)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (newSize.Width < 1 || newSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(newSize), PublicResources.ArgumentOutOfRange);
            if (!scalingMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(scalingMode), PublicResources.EnumOutOfRange(scalingMode));
        }

        // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Local
        #endregion

        #region Copy

        /// <summary>
        /// Cloning without changing pixel format if possible. Pixel format is changed only for indexed custom formats.
        /// </summary>
        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static IReadWriteBitmapData? DoCloneExact(IAsyncContext context, IReadableBitmapData source, WorkingColorSpace workingColorSpace)
        {
            Size size = source.Size;
            var session = new CopySession(context) { SourceRectangle = new Rectangle(Point.Empty, size) };
            Unwrap(ref source, ref session.SourceRectangle);
            session.TargetRectangle = session.SourceRectangle;

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = source is ICustomBitmapData { BackBufferIndependentPixelAccess: true } customBitmapData
                ? customBitmapData.CreateCompatibleBitmapDataFactory.Invoke(session.TargetRectangle.Size, workingColorSpace)
                : BitmapDataFactory.CreateManagedBitmapData(size, source.GetKnownPixelFormat(),
                    source.BackColor, source.AlphaThreshold, workingColorSpace, source.Palette);
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

        /// <summary>
        /// Cloning with arbitrary pixel format and core settings using direct get/set pixels.
        /// NOTE: pixelFormat actually can be invalid here for custom pixel formats, in which case CreateCompatibleBitmapDataFactory must create the clone.
        /// </summary>
        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static IBitmapDataInternal? DoCloneDirect(IAsyncContext context, IReadableBitmapData source, Rectangle sourceRectangle,
            KnownPixelFormat pixelFormat, Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace, Palette? palette)
        {
            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.Size);
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty() || session.TargetRectangle.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            if (palette == null)
            {
                int bpp = pixelFormat.ToBitsPerPixel();
                if (bpp <= 8 && source.Palette?.Entries.Length <= (1 << bpp))
                    palette = backColor == source.Palette!.BackColor && alphaThreshold == source.Palette.AlphaThreshold && workingColorSpace == source.Palette.WorkingColorSpace
                        ? source.Palette
                        : new Palette(source.Palette.Entries, backColor, alphaThreshold, workingColorSpace, null);
            }

            session.Source = source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = source is ICustomBitmapData { BackBufferIndependentPixelAccess: true } customBitmapData && customBitmapData.PixelFormat.AsKnownPixelFormatInternal == pixelFormat
                ? customBitmapData.CreateCompatibleBitmapDataFactory.Invoke(session.TargetRectangle.Size, workingColorSpace)
                : BitmapDataFactory.CreateManagedBitmapData(session.TargetRectangle.Size, pixelFormat.IsValidFormat() ? pixelFormat : source.GetKnownPixelFormat(),
                    backColor, alphaThreshold, workingColorSpace, palette);
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
        private static IBitmapDataInternal? DoCloneWithQuantizer(IAsyncContext context, IReadableBitmapData source, Rectangle sourceRectangle,
            KnownPixelFormat pixelFormat, IQuantizer? quantizer, IDitherer? ditherer = null)
        {
            if (quantizer == null)
            {
                // copying without using a quantizer (even if only a ditherer is specified for a high-bpp pixel format)
                // Note: Not using source.BackColor/AlphaThreshold/Palette so the behavior will be compatible with the other Clone overloads with default parameters
                //       and even with ImageExtensions.ConvertPixelFormat where there are no BackColor/AlphaThreshold for source image
                if (ditherer == null || !pixelFormat.CanBeDithered())
                    return DoCloneDirect(context, source, sourceRectangle, pixelFormat, source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null);

                // here we need to pick a quantizer for the dithering
                int bpp = pixelFormat.ToBitsPerPixel();
                Color32[] paletteEntries = source.Palette?.Entries ?? Reflector.EmptyArray<Color32>();
                quantizer = (bpp <= 8 && paletteEntries.Length > 0 && paletteEntries.Length <= (1 << bpp)
                        ? PredefinedColorsQuantizer.FromCustomPalette(source.Palette!)
                        : PredefinedColorsQuantizer.FromPixelFormat(pixelFormat, source.BackColor, source.AlphaThreshold))
                    .ConfigureColorSpace(source.GetPreferredColorSpaceOrDefault());
            }

            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.Size);
            Unwrap(ref source, ref sourceRectangle);
            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, new Rectangle(Point.Empty, sourceBounds.Size), Point.Empty);
            if (session.SourceRectangle.IsEmpty() || session.TargetRectangle.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(sourceRectangle), PublicResources.ArgumentOutOfRange);

            // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
            IReadableBitmapData initSource = session.SourceRectangle.Size == source.Size
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
                    session.Target = BitmapDataFactory.CreateManagedBitmapData(session.TargetRectangle.Size, pixelFormat,
                        quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.WorkingColorSpace,
                        quantizingSession.Palette);

                    // quantizing without dithering
                    if (ditherer == null)
                        session.PerformCopyWithQuantizer(quantizingSession);
                    else
                    {
                        // quantizing with dithering
                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                        if (canceled = context.IsCancellationRequested)
                            return null;
                        if (ditheringSession == null)
                            throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                        session.PerformCopyWithDithering(quantizingSession, ditheringSession);
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

        private static bool DoCopy(IAsyncContext context, IReadableBitmapData source, IWritableBitmapData target,
            Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer, bool skipTransparent = false, in Array2D<byte> mask = default, Point maskOffset = default)
        {
            var session = new CopySession(context);
            var sourceBounds = new Rectangle(default, source.Size);
            var targetBounds = new Rectangle(default, target.Size);
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (session.SourceRectangle, session.TargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (session.SourceRectangle.IsEmpty() || session.TargetRectangle.IsEmpty())
                return !context.IsCancellationRequested;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            // special handling for same references
            if (ReferenceEquals(source, target))
            {
                // same area without quantizing: nothing to do
                if (quantizer == null && session.SourceRectangle == session.TargetRectangle)
                    return !context.IsCancellationRequested;

                // overlap: clone source
                if (session.SourceRectangle.IntersectsWith(session.TargetRectangle))
                {
                    session.Source = DoCloneDirect(context, source, session.SourceRectangle, source.PixelFormat.AsKnownPixelFormatInternal,
                        source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null);
                    if (context.IsCancellationRequested)
                    {
                        session.Source?.Dispose();
                        return false;
                    }

                    session.SourceRectangle.Location = Point.Empty;
                }
            }

            session.Source ??= source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            session.Target = target as IBitmapDataInternal ?? new BitmapDataWrapper(target, false, true);

            try
            {
                // processing without using a quantizer
                if (quantizer == null)
                {
                    Debug.Assert(!skipTransparent && mask.IsNull, "Skipping transparent source pixels or applying a mask is not expected without quantizing. Handle it if really needed.");
                    session.PerformCopy();
                    return !context.IsCancellationRequested;
                }

                // Using a clipped source for quantizer/ditherer if needed. Note: the CopySession uses the original source for the best performance
                IReadableBitmapData initSource = session.SourceRectangle.Size == source.Size
                    ? source
                    : source.Clip(session.SourceRectangle);

                try
                {
                    context.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                    {
                        if (context.IsCancellationRequested)
                            return false;
                        if (quantizingSession == null)
                            throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                        // quantization without dithering
                        if (ditherer == null)
                        {
                            session.PerformCopyWithQuantizer(quantizingSession, skipTransparent, mask, maskOffset);
                            return !context.IsCancellationRequested;
                        }

                        // quantization with dithering
                        context.Progress?.New(DrawingOperation.InitializingDitherer);
                        using (IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context))
                        {
                            if (context.IsCancellationRequested)
                                return false;
                            if (ditheringSession == null)
                                throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                            session.PerformCopyWithDithering(quantizingSession, ditheringSession, skipTransparent, mask, maskOffset);
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

            return !context.IsCancellationRequested;
        }

        #endregion

        #region Draw

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawInto(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target,
            Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer)
            => source.HasAlpha()
                ? DoDrawWithoutResize(context, source, target, sourceRectangle, targetLocation, quantizer, ditherer)
                : DoCopy(context, source, target, sourceRectangle, targetLocation, quantizer, ditherer);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawInto(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target,
            Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode)
        {
            // no scaling is necessary
            if (sourceRectangle.Size == targetRectangle.Size || scalingMode == ScalingMode.NoScaling)
            {
                return source.HasAlpha()
                    ? DoDrawWithoutResize(context, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer)
                    : DoCopy(context, source, target, sourceRectangle, targetRectangle.Location, quantizer, ditherer);
            }

            return DoDrawWithResize(context, source, target, sourceRectangle, targetRectangle, quantizer, ditherer, scalingMode);
        }

        private static bool DoDrawWithoutResize(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target,
            Rectangle sourceRectangle, Point targetLocation, IQuantizer? quantizer, IDitherer? ditherer)
        {
            Debug.Assert(source.HasAlpha(), "DoCopy could have been called");

            var sourceBounds = new Rectangle(default, source.Size);
            var targetBounds = new Rectangle(default, target.Size);
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetLocation);
            if (actualSourceRectangle.IsEmpty() || actualTargetRectangle.IsEmpty())
                return !context.IsCancellationRequested;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal? sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;
            bool isTwoPass = source.HasMultiLevelAlpha() && (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true);

            // if two pass is needed we create a temp result where we perform blending before quantizing/dithering
            if (isTwoPass)
            {
                var workingColorSpace = quantizer.WorkingColorSpace();
                sessionTarget = DoCloneDirect(context, target, actualTargetRectangle, target.GetPreferredFirstPassPixelFormat(workingColorSpace),
                    target.BackColor, target.AlphaThreshold, workingColorSpace, null);
                if (context.IsCancellationRequested)
                {
                    sessionTarget?.Dispose();
                    return false;
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
                    return !context.IsCancellationRequested;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = DoCloneDirect(context, source, actualSourceRectangle, source.PixelFormat.AsKnownPixelFormatInternal,
                        source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null);
                    if (context.IsCancellationRequested)
                    {
                        sessionSource?.Dispose();
                        return false;
                    }

                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            sessionSource ??= source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);

            try
            {
                var session = new CopySession(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle);
                if (!isTwoPass)
                {
                    session.PerformDraw(quantizer, ditherer);
                    return !context.IsCancellationRequested;
                }

                // first pass: performing blending into transient result
                session.PerformDrawDirect();

                // second pass: copying the blended transient result to the actual target
                if (context.IsCancellationRequested)
                    return false;
                return DoCopy(context, sessionTarget!, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
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
        private static bool DoDrawWithResize(IAsyncContext context, IReadableBitmapData source, IReadWriteBitmapData target,
            Rectangle sourceRectangle, Rectangle targetRectangle, IQuantizer? quantizer, IDitherer? ditherer, ScalingMode scalingMode)
        {
            Debug.Assert(sourceRectangle.Size != targetRectangle.Size || scalingMode == ScalingMode.NoScaling, $"{nameof(DoDrawWithoutResize)} could have been called");

            var sourceBounds = new Rectangle(default, source.Size);
            var targetBounds = new Rectangle(default, target.Size);
            Unwrap(ref source, ref sourceBounds);
            Unwrap(ref target, ref targetBounds);

            (Rectangle actualSourceRectangle, Rectangle actualTargetRectangle) = GetActualRectangles(sourceBounds, sourceRectangle, targetBounds, targetRectangle);
            if (actualSourceRectangle.IsEmpty() || actualTargetRectangle.IsEmpty())
                return !context.IsCancellationRequested;

            AdjustQuantizerAndDitherer(target, ref quantizer, ref ditherer);

            IBitmapDataInternal? sessionTarget;
            Rectangle sessionTargetRectangle = actualTargetRectangle;
            bool targetCloned = false;

            // note: when resizing, we cannot trick the quantizer/ditherer with a single-bit alpha source because the source is needed to be resized
            bool isTwoPass = quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true;

            // if two pass is needed we create a temp result where we perform resize (with or without blending) before quantizing/dithering
            if (isTwoPass)
            {
                WorkingColorSpace workingColorSpace = quantizer.WorkingColorSpace();
                KnownPixelFormat sessionTargetPixelFormat = target.GetPreferredFirstPassPixelFormat(workingColorSpace);
                sessionTarget = source.HasMultiLevelAlpha()
                    ? DoCloneDirect(context, target, actualTargetRectangle, sessionTargetPixelFormat,
                        target.BackColor, target.AlphaThreshold, workingColorSpace, null)
                    : BitmapDataFactory.CreateManagedBitmapData(sessionTargetRectangle.Size, sessionTargetPixelFormat,
                        target.BackColor, target.AlphaThreshold, workingColorSpace, null);
                if (context.IsCancellationRequested)
                {
                    sessionTarget?.Dispose();
                    return false;
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
                    return !context.IsCancellationRequested;

                // overlap: clone source
                if (actualSourceRectangle.IntersectsWith(actualTargetRectangle))
                {
                    sessionSource = DoCloneDirect(context, source, actualSourceRectangle, source.PixelFormat.AsKnownPixelFormatInternal,
                        source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, null);
                    if (context.IsCancellationRequested)
                    {
                        sessionSource?.Dispose();
                        return false;
                    }

                    actualSourceRectangle.Location = Point.Empty;
                }
            }

            sessionSource ??= source as IBitmapDataInternal ?? new BitmapDataWrapper(source, true, false);
            try
            {
                bool linear = (quantizer?.WorkingColorSpace() ?? target.GetPreferredColorSpace()) == WorkingColorSpace.Linear;
                if (scalingMode == ScalingMode.NearestNeighbor)
                {
                    var session = new ResizingSessionNearestNeighbor(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle, linear);
                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return !context.IsCancellationRequested;
                    }

                    // first pass: performing resizing into a transient result
                    session.PerformResizeDirect();
                }
                else
                {
                    using var session = ResizingSessionInterpolated.Create(context, sessionSource, sessionTarget!, actualSourceRectangle, sessionTargetRectangle, scalingMode, linear);
                    if (context.IsCancellationRequested)
                        return false;

                    if (!isTwoPass)
                    {
                        session.PerformResize(quantizer, ditherer);
                        return !context.IsCancellationRequested;
                    }

                    // first pass: performing blending into transient result
                    session.PerformResizeDirect();
                }

                if (context.IsCancellationRequested)
                    return false;

                // second pass: copying the possibly blended transient result to the actual target with quantizing/dithering
                DoCopy(context, sessionTarget!, target, sessionTargetRectangle, actualTargetRectangle.Location, quantizer, ditherer, true);
                return !context.IsCancellationRequested;
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
            Rectangle actualSourceRectangle = sourceRectangle.IntersectSafe(sourceBounds);
            if (actualSourceRectangle.IsEmpty())
                return default;
            targetLocation.Offset(targetBounds.Location);
            Rectangle targetRectangle = new Rectangle(targetLocation, sourceRectangle.Size);
            Rectangle actualTargetRectangle = targetRectangle.IntersectSafe(targetBounds);
            if (actualTargetRectangle.IsEmpty())
                return default;

            // adjusting source by clipped target
            if (targetRectangle != actualTargetRectangle)
            {
                int x = actualTargetRectangle.X - targetRectangle.X + sourceRectangle.X;
                int y = actualTargetRectangle.Y - targetRectangle.Y + sourceRectangle.Y;
                actualSourceRectangle = actualSourceRectangle.IntersectSafe(new Rectangle(x, y, actualTargetRectangle.Width, actualTargetRectangle.Height));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = actualSourceRectangle.X - sourceRectangle.X + targetRectangle.X;
                int y = actualSourceRectangle.Y - sourceRectangle.Y + targetRectangle.Y;
                actualTargetRectangle = actualTargetRectangle.IntersectSafe(new Rectangle(x, y, actualSourceRectangle.Width, actualSourceRectangle.Height));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        private static (Rectangle Source, Rectangle Target) GetActualRectangles(Rectangle sourceBounds, Rectangle sourceRectangle, Rectangle targetBounds, Rectangle targetRectangle)
        {
            sourceRectangle.Offset(sourceBounds.Location);
            Rectangle actualSourceRectangle = sourceRectangle.IntersectSafe(sourceBounds);
            if (actualSourceRectangle.IsEmpty())
                return default;
            targetRectangle.Offset(targetBounds.Location);
            Rectangle actualTargetRectangle = targetRectangle.IntersectSafe(targetBounds);
            if (actualTargetRectangle.IsEmpty())
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
                actualSourceRectangle = actualSourceRectangle.IntersectSafe(new Rectangle(x, y, w, h));
            }

            // adjusting target by clipped source
            if (sourceRectangle != actualSourceRectangle)
            {
                int x = (int)MathF.Round((actualSourceRectangle.X - sourceRectangle.X) / widthRatio + targetRectangle.X);
                int y = (int)MathF.Round((actualSourceRectangle.Y - sourceRectangle.Y) / heightRatio + targetRectangle.Y);
                int w = (int)MathF.Round(actualSourceRectangle.Width / widthRatio);
                int h = (int)MathF.Round(actualSourceRectangle.Height / heightRatio);
                actualTargetRectangle = actualTargetRectangle.IntersectSafe(new Rectangle(x, y, w, h));
            }

            return (actualSourceRectangle, actualTargetRectangle);
        }

        #endregion

        #region GetColors

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "False alarm, up to one of the typeof comparisons are left in the release build")]
        private static ICollection<T> DoGetColors<T>(IAsyncContext context, IReadableBitmapData bitmapData, int maxColors)
            where T : unmanaged
        {
            if (maxColors < 0)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentOutOfRange);
            if (maxColors == 0)
                maxColors = bitmapData.Palette?.Count ?? bitmapData.PixelFormat.GetColorsLimit();

            var colors = new HashSet<T>();
            var data = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, false);

            try
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, data.Height);
                IBitmapDataRowInternal line = data.GetRowCached(0);

                do
                {
                    if (context.IsCancellationRequested)
                        return Reflector.EmptyArray<T>();
                    for (int x = 0; x < data.Width; x++)
                    {
                        // The unnecessary branches are optimized away in Release build
                        if (typeof(T) == typeof(Color32))
                        {
                            Color32 c = line.DoGetColor32(x);
                            colors.Add((T)(object)(c.A == 0 ? default : c));
                        }
                        else if (typeof(T) == typeof(PColor32))
                        {
                            PColor32 c = line.DoGetPColor32(x);
                            colors.Add((T)(object)(c.A == 0 ? default : c));
                        }
                        else if (typeof(T) == typeof(Color64))
                        {
                            Color64 c = line.DoGetColor64(x);
                            colors.Add((T)(object)(c.A == 0 ? default : c));
                        }
                        else if (typeof(T) == typeof(PColor64))
                        {
                            PColor64 c = line.DoGetPColor64(x);
                            colors.Add((T)(object)(c.A == 0 ? default : c));
                        }
                        else if (typeof(T) == typeof(ColorF))
                        {
                            ColorF c = line.DoGetColorF(x).Clip();
                            colors.Add((T)(object)(c.A == 0f ? default : c));
                        }
                        else if (typeof(T) == typeof(PColorF))
                        {
                            PColorF c = line.DoGetPColorF(x).Clip();
                            colors.Add((T)(object)(c.A == 0f ? default : c));
                        }
                        else
                            throw new InvalidOperationException($"Unexpected T: {typeof(T)}");

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
            var pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.AsKnownPixelFormatInternal switch
            {
                // Special cases for possible compact counting (when the actual pixel format is smaller than the preferred color)
                KnownPixelFormat.Format24bppRgb when bitmapData.RowSize >= bitmapData.Width * 3 => GetColorCount<Color24>(context, bitmapData),
                KnownPixelFormat.Format16bppArgb1555 when bitmapData.RowSize >= bitmapData.Width << 1 => GetColorCount<Color16Argb1555>(context, bitmapData),
                KnownPixelFormat.Format16bppRgb555 when bitmapData.RowSize >= bitmapData.Width << 1 => GetColorCount<Color16Rgb555>(context, bitmapData),
                KnownPixelFormat.Format16bppRgb565 when bitmapData.RowSize >= bitmapData.Width << 1 => GetColorCount<Color16Rgb565>(context, bitmapData),
                KnownPixelFormat.Format8bppGrayScale when bitmapData.RowSize >= bitmapData.Width => GetColorCount<Gray8>(context, bitmapData),
                KnownPixelFormat.Format16bppGrayScale when bitmapData.RowSize >= bitmapData.Width << 1 => GetColorCount<Gray16>(context, bitmapData),
                KnownPixelFormat.Format32bppGrayScale when bitmapData.RowSize >= bitmapData.Width << 2 => GetColorCount<GrayF>(context, bitmapData),
                KnownPixelFormat.Format48bppRgb when bitmapData.RowSize >= bitmapData.Width * 6 => GetColorCount<Color48>(context, bitmapData),
                KnownPixelFormat.Format96bppRgb when bitmapData.RowSize >= bitmapData.Width * 12 => GetColorCount<RgbF>(context, bitmapData),
                _ => pixelFormat.Prefers128BitColors ? pixelFormat.HasPremultipliedAlpha ? DoGetColors<PColorF>(context, bitmapData, 0).Count : DoGetColors<ColorF>(context, bitmapData, 0).Count
                    : pixelFormat.Prefers64BitColors ? pixelFormat.HasPremultipliedAlpha ? DoGetColors<PColor64>(context, bitmapData, 0).Count : DoGetColors<Color64>(context, bitmapData, 0).Count
                    : pixelFormat.HasPremultipliedAlpha ? DoGetColors<PColor32>(context, bitmapData, 0).Count : DoGetColors<Color32>(context, bitmapData, 0).Count
            };
        }

        [SecuritySafeCritical]
        private static int GetColorCount<T>(IAsyncContext context, IReadableBitmapData bitmapData) where T : unmanaged
        {
            Debug.Assert(bitmapData.PixelFormat.IsKnownFormat);
            Debug.Assert(typeof(IEquatable<T>).IsAssignableFrom(typeof(T)), "T should implement IEquatable<T>");
            var colors = new HashSet<T>();
            var data = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, false);
            try
            {
                int max = bitmapData.PixelFormat.GetColorsLimit();
                context.Progress?.New(DrawingOperation.ProcessingPixels, data.Height);
                IBitmapDataRowInternal line = data.GetRowCached(0);

                do
                {
                    if (context.IsCancellationRequested)
                        return default;
                    for (int x = 0; x < data.Width; x++)
                    {
                        T color = line.DoReadRaw<T>(x);

                        // The JIT compiler will optimize these branches in Release build
                        if (color is Color16Argb1555 { A: 0 })
                            color = default;
                        if (color is RgbF rgbF)
                            color = (T)(object)rgbF.Clip();
                        if (color is GrayF grayF)
                            color = (T)(object)grayF.Clip();
                        colors.Add(color);
                        if (colors.Count == max)
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
            var srcRect = new Rectangle(Point.Empty, bitmapData.Size);
            Color32 transparentColor = bitmapData[bitmapData.Height - 1][0];
            if (transparentColor.A < Byte.MaxValue)
                return DoCloneDirect(context, bitmapData, srcRect, KnownPixelFormat.Format32bppArgb, default, 128, WorkingColorSpace.Default, null);
            return DoCloneWithQuantizer(context, bitmapData, srcRect, KnownPixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => c == transparentColor ? default : c));
        }

        private static IReadWriteBitmapData? DoToTransparent(IAsyncContext context, IReadableBitmapData bitmapData, Color32 transparentColor)
        {
            var srcRect = new Rectangle(Point.Empty, bitmapData.Size);
            if (transparentColor.A == 0)
                return DoCloneDirect(context, bitmapData, srcRect, KnownPixelFormat.Format32bppArgb, default, 128, WorkingColorSpace.Default, null);
            return DoCloneWithQuantizer(context, bitmapData, srcRect, KnownPixelFormat.Format32bppArgb,
                PredefinedColorsQuantizer.FromCustomFunction(c => c == transparentColor ? default : c));
        }

        #endregion

        #region Resize

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        private static IReadWriteBitmapData? DoResize(IAsyncContext context, IReadableBitmapData bitmapData, Size newSize, ScalingMode scalingMode, bool keepAspectRatio)
        {
            Size sourceSize = bitmapData.Size;
            Rectangle targetRectangle;
            if (keepAspectRatio && newSize != sourceSize)
            {
                float ratio = Math.Min((float)newSize.Width / sourceSize.Width, (float)newSize.Height / sourceSize.Height);
                var targetSize = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
                var targetLocation = new Point((newSize.Width >> 1) - (targetSize.Width >> 1), (newSize.Height >> 1) - (targetSize.Height >> 1));
                targetRectangle = new Rectangle(targetLocation, targetSize);
            }
            else
                targetRectangle = new Rectangle(Point.Empty, newSize);

            if (context.IsCancellationRequested)
                return null;

            bool canceled = false;
            bool isLinear = bitmapData.LinearBlending();
            PixelFormatInfo sourceFormat = bitmapData.PixelFormat;
            var targetFormat = sourceFormat.Prefers128BitColors ? isLinear ? KnownPixelFormat.Format128bppPRgba : KnownPixelFormat.Format128bppRgba
                : sourceFormat.Prefers64BitColors ? isLinear ? KnownPixelFormat.Format64bppArgb : KnownPixelFormat.Format64bppPArgb
                : isLinear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb;
            IReadWriteBitmapData? result = BitmapDataFactory.CreateBitmapData(newSize, targetFormat, bitmapData.WorkingColorSpace);
            try
            {
                DoDrawInto(context, bitmapData, result, new Rectangle(Point.Empty, sourceSize), targetRectangle, null, null, scalingMode);
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

        #region Save

        private static void DoSave(IAsyncContext context, IReadableBitmapData bitmapData, Stream stream)
        {
            Size size = bitmapData.Size;
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
