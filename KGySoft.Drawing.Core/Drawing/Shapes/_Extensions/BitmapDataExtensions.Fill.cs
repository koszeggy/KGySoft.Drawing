#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Fill.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

#region Used Aliases

#if NETFRAMEWORK
using Brush = KGySoft.Drawing.Shapes.Brush;
using SolidBrush = KGySoft.Drawing.Shapes.SolidBrush;
#endif

#endregion

#endregion

#region Suppressions

// ReSharper disable PossibleMultipleEnumeration - Validation methods just check null. Note: ReSharper 2024.2.6 simply ignores NoEnumerationAttribute added to an Annotations.cs file.

#endregion

namespace KGySoft.Drawing.Shapes
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region Polygon

        #region Sync

        #region Default Context

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            DoFillPolygon(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            DoFillPolygon(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to fill a polygon with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPolygon">EndFillPolygon</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a polygon with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPolygon">EndFillPolygon</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a polygon with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPolygon">EndFillPolygon</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a polygon with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPolygon">EndFillPolygon</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPolygon">BeginFillPolygon</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPolygonAsync">FillPolygonAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        public static bool EndFillPolygon(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPolygon));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the polygon to fill.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a polygon with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the polygon.</param>
        /// <param name="points">The vertices of the polygon to draw.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>Every pair of two consecutive points specifies a side of the polygon. In addition, if the last point and the first point do not coincide, they specify the last side of the polygon.</para>
        /// <para>This method does not use optimized shortcuts. If the same polygon is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the polygon to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Rectangle

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner.</param>
        /// <param name="y">The y-coordinate of the upper-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>When no shortcut can be used and the same rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => FillRectangle(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>When no shortcut can be used and the same rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA fill
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.FillRectangle(AsyncHelper.DefaultContext, bitmapData, rectangle, color);
                return;
            }

            DoFillRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner.</param>
        /// <param name="y">The y-coordinate of the upper-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>When no shortcut can be used and the same rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => FillRectangle(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>When no shortcut can be used and the same rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA fill, if rectangle is integer
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                {
                    DirectDrawer.FillRectangle(AsyncHelper.DefaultContext, bitmapData, rect, color);
                    return;
                }
            }

            DoFillRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see FillRectangleShortcutTest in performance tests).

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                return AsyncHelper.DoOperationSynchronously(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rectangle, color), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.DoOperationSynchronously(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the specified <paramref name="brush"/>
        /// is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle rectangle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                return AsyncHelper.DoOperationSynchronously(ctx => solidBrush.FillRectangle(ctx, bitmapData, rectangle), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// the specified <paramref name="brush"/> is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.DoOperationSynchronously(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                return DirectDrawer.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rectangle, color);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return DirectDrawer.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rect, color);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the specified <paramref name="brush"/>
        /// is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                return solidBrush.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rectangle);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// the specified <paramref name="brush"/> is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return solidBrush.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rect);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to fill a rectangle with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRectangle">EndFillRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                return AsyncHelper.BeginOperation(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rectangle, color), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rectangle with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRectangle">EndFillRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.BeginOperation(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rectangle with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the specified <paramref name="brush"/>
        /// is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRectangle">EndFillRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                return AsyncHelper.BeginOperation(ctx => solidBrush.FillRectangle(ctx, bitmapData, rectangle), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rectangle with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// the specified <paramref name="brush"/> is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRectangle">EndFillRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.BeginOperation(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRectangle">BeginFillRectangle</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRectangleAsync">FillRectangleAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        public static bool EndFillRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillRectangle));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when <paramref name="drawingOptions"/> is <see langword="null"/>
        /// and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                return AsyncHelper.DoOperationAsync(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rectangle, color), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rectangle to fill.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// and <paramref name="drawingOptions"/> is <see langword="null"/> and the specified <paramref name="color"/> is opaque, or when <paramref name="drawingOptions"/> specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.DoOperationAsync(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the specified <paramref name="brush"/>
        /// is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                return AsyncHelper.DoOperationAsync(ctx => solidBrush.FillRectangle(ctx, bitmapData, rectangle), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rectangle with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rectangle.</param>
        /// <param name="rectangle">The rectangle to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rectangle, the right/bottom values of the coordinates are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method tries to use a shortcut to fill the rectangle directly, which is faster than creating a <see cref="Path"/> and adding the rectangle to it. A shortcut is possible when the location and size are integer values,
        /// the specified <paramref name="brush"/> is a solid brush with an opaque color, and if <paramref name="drawingOptions"/> is either <see langword="null"/>, or it specifies that no anti-aliasing and no alpha blending is required,
        /// the transformation is the identity matrix, and neither <see cref="DrawingOptions.Quantizer"/> nor <see cref="DrawingOptions.Ditherer"/> is specified.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = rectangle.TruncateChecked();
                if (rect == rectangle)
                    return AsyncHelper.DoOperationAsync(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Ellipse
        // NOTE: Unlike the Rectangle methods, this section have no shortcuts because it wouldn't produce the same result as the Path-based fill,
        // so most of the overloads are just for symmetry reasons, and for possible future compatibility in case of introducing shortcuts.

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => FillEllipse(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillEllipse(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => FillEllipse(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillEllipse(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to fill an ellipse with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillEllipse">EndFillEllipse</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill an ellipse with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillEllipse">EndFillEllipse</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill an ellipse with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillEllipse">EndFillEllipse</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill an ellipse with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillEllipse">EndFillEllipse</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillEllipse">BeginFillEllipse</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillEllipseAsync">FillEllipseAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        public static bool EndFillEllipse(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillEllipse));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the ellipse to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills an ellipse with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the ellipse.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling an ellipse, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same ellipse is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the ellipse to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Pie

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => FillPie(bitmapData, color, new Rectangle(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillPie(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="width">The width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="height">The height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => FillPie(bitmapData, color, new RectangleF(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillPie(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to fill a pie shape with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPie">EndFillPie</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a pie shape with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPie">EndFillPie</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a pie shape with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPie">EndFillPie</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a pie shape with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPie">EndFillPie</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPie">BeginFillPie</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPieAsync">FillPieAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        public static bool EndFillPie(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPie));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the pie to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a pie shape with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the pie shape.</param>
        /// <param name="bounds">The bounding rectangle that defines the ellipse from which the pie shape comes.</param>
        /// <param name="startAngle">The starting angle of the pie shape, measured in degrees clockwise from the x-axis.</param>
        /// <param name="sweepAngle">The sweep angle of the pie shape, measured in degrees clockwise from <paramref name="startAngle"/>.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>The pie shape is defined by an arc of an ellipse and the two radial lines that intersect with the endpoints of the arc.</para>
        /// <para>This method does not use optimized shortcuts. If the same pie shape is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the pie to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region RoundedRectangle

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, int cornerRadius, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), cornerRadius, drawingOptions);

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float cornerRadius, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), cornerRadius, drawingOptions);

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="width">The width of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="height">The height of the bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if <paramref name="width"/> or <paramref name="height"/> is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying the same corner radius to all corners,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/>, applying a custom corner radius to each corner,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying the same corner radius to all corners,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying a custom corner radius to each corner,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying the same corner radius to all corners,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/>, applying a custom corner radius to each corner,
        /// and using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillRoundedRectangle">EndFillRoundedRectangle</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillRoundedRectangle">BeginFillRoundedRectangle</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillRoundedRectangleAsync">FillRoundedRectangleAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        public static bool EndFillRoundedRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillRoundedRectangle));

        #endregion

        #region Async TAP
#if !NET35

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <paramref name="color"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the rounded rectangle to fill.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple filled rectangle will be drawn.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified <see cref="Brush"/> asynchronously, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the rounded rectangle.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>When filling a rounded rectangle, the right/bottom values of the bounding rectangle are exclusive, so if the width or height is zero, then nothing is drawn.</para>
        /// <para>If a corner radius parameter is negative, its absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// <para>This method does not use optimized shortcuts. If the same rounded rectangle is filled repeatedly, creating a <see cref="Path"/> with <see cref="Path.PreferCaching"/> enabled and adding the rounded rectangle to it can provide a better performance.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="brush"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Path

        /// <summary>
        /// Fills a <see cref="Path"/> with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the path to fill.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <note>This method adjusts the degree of parallelization automatically, blocks the caller, and does not support cancellation or reporting progress. You can use the overloads that have
        /// a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_ParallelConfig.htm" target="_blank">ParallelConfig</a> parameter to configure these, while still executing the method synchronously. Alternatively, use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            DoFillPath(AsyncHelper.DefaultContext, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a <see cref="Path"/> with the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the path to fill.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use. If <see langword="null"/>, then the default options are used.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a <see cref="Path"/> with the specified <see cref="Brush"/>.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the <see cref="Path"/>.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="parallelConfig">The configuration of the operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface.
        /// If <see langword="null"/>, then the degree of parallelization is configured automatically. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// of the <paramref name="parallelConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <note>This method blocks the caller as it executes synchronously, though the <paramref name="parallelConfig"/> parameter allows configuring the degree of parallelism, cancellation and progress reporting. Use
        /// the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> or <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see>
        /// (in .NET Framework 4.0 and above) methods to perform the operation asynchronously.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="parallelConfig"/> was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        /// <summary>
        /// Fills a <see cref="Path"/> with the specified <paramref name="color"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="color">The color of the path to fill.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            return DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Fills a <see cref="Path"/> with the specified <see cref="Brush"/>, using a <paramref name="context"/> that may belong to a higher level, possibly asynchronous operation.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="context">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncContext.htm">IAsyncContext</a> instance
        /// that contains information for asynchronous processing about the current operation.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the <see cref="Path"/>.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
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
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default);
        }

        /// <summary>
        /// Begins to fill a <see cref="Path"/> with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the path to fill.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPath">EndFillPath</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static IAsyncResult BeginFillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.BeginOperation(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a <see cref="Path"/> with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the <see cref="Path"/>.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> methods.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndFillPath">EndFillPath</see> method.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static IAsyncResult BeginFillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.BeginOperation(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.BeginFillPath">BeginFillPath</see> methods to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="O:KGySoft.Drawing.Shapes.BitmapDataExtensions.FillPathAsync">FillPathAsync</see> methods instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns><see langword="true"/>, if the operation completed successfully.
        /// <br/><see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was set to <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <see cref="DrawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="OperationCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in the <c>asyncConfig</c> parameter was <see langword="true"/>.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool EndFillPath(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPath));

#if !NET35
        /// <summary>
        /// Begins to fill a <see cref="Path"/> with the specified <paramref name="color"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="color">The color of the path to fill.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Task<bool> FillPathAsync(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        /// <summary>
        /// Begins to fill a <see cref="Path"/> with the specified <see cref="Brush"/> asynchronously.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IReadWriteBitmapData"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> to use for filling the <see cref="Path"/>.</param>
        /// <param name="path">The <see cref="Path"/> instance to fill.</param>
        /// <param name="drawingOptions">A <see cref="DrawingOptions"/> instance that specifies the drawing options to use.
        /// If <see langword="null"/>, then the default options are used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is <see langword="true"/>, if the operation completed successfully,
        /// or <see langword="false"/>, if the operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property in <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>If the <see cref="DrawingOptions.Transformation"/> property of <paramref name="drawingOptions"/> is not the identity matrix, then the path region is not cached, even if <see cref="Path.PreferCaching"/> is enabled.
        /// To improve the performance of filling transformed paths repeatedly, apply the transformations to the <paramref name="path"/> instance instead, and use the identity matrix in <paramref name="drawingOptions"/>.</para>
        /// <para>This method is not a blocking call even if the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is 1.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bitmapData"/>, <paramref name="brush"/>, or <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">The coordinates (after a possible transformation specified in <paramref name="drawingOptions"/>) are outside the bounds of an <see cref="int">int</see> value.</exception>
        /// <exception cref="TaskCanceledException">The operation has been canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property
        /// in <paramref name="asyncConfig"/> was <see langword="true"/>. This exception is thrown when the result is awaited.</exception>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Task<bool> FillPathAsync(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, Brush brush)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, Brush brush, IEnumerable points)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IReadWriteBitmapData bitmapData, Brush brush, Path path)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
        }

        #endregion

        #region DoFillXXX

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillPolygon(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddPolygon(points.Select(p => (PointF)p)), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillPolygon(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddPolygon(points), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddRectangle(rectangle), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillEllipse(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddEllipse(rectangle), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillPie(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, float startAngle, float sweepAngle, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddPie(rectangle, startAngle, sweepAngle), brush, drawingOptions);


        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillRoundedRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, float cornerRadius, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddRoundedRectangle(rectangle, cornerRadius), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillRoundedRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddRoundedRectangle(rectangle, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft), brush, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, Brush brush, DrawingOptions drawingOptions)
        {
            if (!drawingOptions.IsIdentityTransform)
            {
                path = Path.Transform(path, drawingOptions.Transformation);
                path.PreferCaching = false;
            }

            return brush.FillPath(context, bitmapData, path, drawingOptions);
        }

        #endregion

        #endregion

        #endregion
    }
}
