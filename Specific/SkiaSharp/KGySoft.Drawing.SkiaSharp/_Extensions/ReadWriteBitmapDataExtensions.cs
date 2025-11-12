#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ReadWriteBitmapDataExtensions.cs
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

#region Used Namespaces

using System;
using System.Drawing;
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using SkiaSharp;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Pen = KGySoft.Drawing.Shapes.Pen;

#endregion

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadWriteBitmapData"/> type.
    /// </summary>
    public static class ReadWriteBitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region DrawTextOutline

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate float overloads for convenience reasons.

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, float x, float y, DrawingOptions? drawingOptions = null)
            => DrawTextOutline(bitmapData, color, text, font, new PointF(x, y), drawingOptions);

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, text, font);
            DoDrawTextOutline(AsyncHelper.DefaultContext, bitmapData, new Pen(color), text, font, location, drawingOptions);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated to be consistent with KGySoft.Drawing.Shapes.BitmapDataExtensions

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
            => DrawTextOutline(bitmapData, new Pen(color), text, font, location, drawingOptions, parallelConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, Pen pen, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, text, font, pen);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawTextOutline(ctx, bitmapData, pen, text, font, location,  drawingOptions), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
            => DrawTextOutline(bitmapData, context, new Pen(color), text, font, location, drawingOptions);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/>,
        /// using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawTextOutline(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, text, font, pen);
            return DoDrawTextOutline(context ?? AsyncHelper.DefaultContext, bitmapData, pen, text, font, location, drawingOptions);
        }

        #endregion

        #endregion

        #region Async

        /// <summary>
        /// Draws the one-pixel wide outline of a text with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the text outline to draw.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => DrawTextOutlineAsync(bitmapData, new Pen(color), text, font, location, drawingOptions, asyncConfig);

        /// <summary>
        /// Draws the outline of a text with the specified <see cref="Pen"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> that determines the characteristics of the text outline.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text, not counting the width of the outline.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method draws only the outline of a text. To draw a text with filled characters, use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> methods instead.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="pen"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextOutlineAsync(this IReadWriteBitmapData bitmapData, Pen pen, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, text, font, pen);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawTextOutline(ctx, bitmapData, pen, text, font, location, drawingOptions), asyncConfig);
        }

        #endregion

        #endregion

        #region DrawText

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate float overloads for convenience reasons.

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, float x, float y, DrawingOptions? drawingOptions = null)
            => DrawText(bitmapData, color, text, font, new PointF(x, y), drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static void DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, text, font);
            DoDrawText(AsyncHelper.DefaultContext, bitmapData, Brush.CreateSolid(color), text, font, location, drawingOptions);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated to be consistent with KGySoft.Drawing.Shapes.BitmapDataExtensions

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
            => DrawText(bitmapData, Brush.CreateSolid(color), text, font, location, drawingOptions, parallelConfig);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see> overloads to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, Brush brush, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, text, font, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawText(ctx, bitmapData, brush, text, font, location, drawingOptions), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Draws a text, filling the characters with a solid brush of the specified <paramref name="color"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
            => DrawText(bitmapData, context, Brush.CreateSolid(color), text, font, location, drawingOptions);

        /// <summary>
        /// Draws a text, filling the characters with the specified <see cref="Brush"/>,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawText">DrawText</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPath">FillPath</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPath">DrawPath</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method blocks the caller thread but if <paramref name="context"/> belongs to an async top level method, then the execution may already run
        /// on a pool thread. Degree of parallelism, the ability of cancellation and reporting progress depend on how these were configured at the top level method.
        /// To reconfigure the degree of parallelism of an existing context, you can use the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncContextWrapper.htm">AsyncContextWrapper</a> class.</para>
        /// <para>Alternatively, you can use this method to specify the degree of parallelism for synchronous execution. For example, by
        /// passing <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncHelper_SingleThreadContext.htm">AsyncHelper.SingleThreadContext</a> to the <paramref name="context"/> parameter
        /// the method will be forced to use a single thread only.</para>
        /// <para>When reporting progress, this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.</para>
        /// <note type="tip">See the <strong>Examples</strong> section of the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_AsyncHelper.htm">AsyncHelper</a>
        /// class for details about how to create a context for possibly async top level methods.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static bool DrawText(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, text, font, brush);
            return DoDrawText(context ?? AsyncHelper.DefaultContext, bitmapData, brush, text, font, location, drawingOptions);
        }

        #endregion

        #endregion

        #region Async

        /// <summary>
        /// Draws a text asynchronously, filling the characters with a solid brush of the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the solid brush to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Color32 color, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
            => DrawTextAsync(bitmapData, Brush.CreateSolid(color), text, font, location, drawingOptions, asyncConfig);

        /// <summary>
        /// Draws a text asynchronously, filling the characters with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to draw the text with.</param>
        /// <param name="text">A <see cref="string">string</see> that represents the text to draw.</param>
        /// <param name="font">The <see cref="SKFont"/> that defines the typeface, size, and style of the text.</param>
        /// <param name="location">The location of the upper-left corner of the text's bounding rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://koszeggy.github.io/docs/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method fills the characters of the text. To draw the outline of a text, you can use the <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutline">DrawTextOutline</see> methods.</para>
        /// <note type="tip">To draw a text with both filled and outlined characters, instead of calling <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextAsync">DrawTextAsync</see>
        /// and <see cref="O:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions.DrawTextOutlineAsync">DrawTextOutlineAsync</see> consecutively, you can achieve a better performance by creating a <see cref="Path"/> once, and then calling
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> and <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.DrawPathAsync">DrawPathAsync</see> methods.
        /// You can convert the specified <paramref name="text"/> to a <see cref="Path"/> by using the <see cref="SKFont.GetTextPath(string,SKPoint)">SKFont.GetTextPath</see> and <see cref="SKPathExtensions.ToPath">SKPathExtensions.ToPath</see> methods.
        /// If you draw the text without antialiasing, it is recommended to set the <see cref="DrawingOptions.FastThinLines"/> to <see langword="false"/> in <paramref name="drawingOptions"/>
        /// so the outline correctly aligns to the filled area.</note>
        /// <para>When <see cref="DrawingOptions.AntiAliasing"/> is set in <paramref name="drawingOptions"/> and you use non-monochromatic colors, it is recommended to specify <see cref="WorkingColorSpace.Linear"/>
        /// color space when you obtain the <see cref="IReadWriteBitmapData"/> instance, especially for small fonts; otherwise, the quality of the drawn text can be poor if alpha colors are blended in the sRGB color space.
        /// See more details at the <strong>Examples</strong> section of the <see cref="WorkingColorSpace"/> enumeration.</para>
        /// <para>This method is not a blocking call even if the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="text"/> or <paramref name="font"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> DrawTextAsync(this IReadWriteBitmapData bitmapData, Brush brush, string text, SKFont font, PointF location, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, text, font, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawText(ctx, bitmapData, brush, text, font, location, drawingOptions), asyncConfig);
        }

        #endregion

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, string text, SKFont font)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (text == null)
                throw new ArgumentNullException(nameof(text), PublicResources.ArgumentNull);
            if (font == null)
                throw new ArgumentNullException(nameof(font), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, string text, SKFont font, Pen pen)
        {
            ValidateArguments(bitmapData, text, font);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, string text, SKFont font, Brush brush)
        {
            ValidateArguments(bitmapData, text, font);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
        }

        #endregion

        #region DoDraw...

        private static bool DoDrawTextOutline(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, string text, SKFont font, PointF location, DrawingOptions? drawingOptions)
        {
            // NOTE: GetTextPath interprets the y coordinate as the baseline, so we need to offset it by font.Size, which is in pixels.
            Path path;
            using (SKPath skiaPath = font.GetTextPath(text, new SKPoint(location.X, location.Y + font.Size)))
            {
                path = skiaPath.ToPath();
                path.PreferCaching = false;
            }

            return path.IsEmpty || !context.IsCancellationRequested && bitmapData.DrawPath(context, pen, path, drawingOptions);
        }

        private static bool DoDrawText(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, string text, SKFont font, PointF location, DrawingOptions? drawingOptions)
        {
            // NOTE: GetTextPath interprets the y coordinate as the baseline, so we need to offset it by font.Size, which is in pixels.
            Path path;
            using (SKPath skiaPath = font.GetTextPath(text, new SKPoint(location.X, location.Y + font.Size)))
            {
                path = skiaPath.ToPath();
                path.PreferCaching = false;
            }

            return path.IsEmpty || !context.IsCancellationRequested && bitmapData.FillPath(context, brush, path, drawingOptions);
        }

        #endregion

        #endregion

        #endregion
    }
}
