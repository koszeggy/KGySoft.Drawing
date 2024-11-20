#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.FillShape.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

#endregion

#region Suppressions

// ReSharper disable PossibleMultipleEnumeration - Validation methods just check null. Note: ReSharper 2024.2.6 simply ignores NoEnumerationAttribute added to an Annotations.cs file.

#endregion

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region Polygon

        #region Sync

        #region Default Context

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            DoFillPolygon(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            DoFillPolygon(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        // Into remarks: when shortcutting, the operation cannot be cancelled, there is no report progress, and we always return true. To avoid that, use DrawPath instead.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return DoFillPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPolygon(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.BeginOperation(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndFillPolygon(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPolygon));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, new SolidBrush(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillPolygonAsync(this IReadWriteBitmapData bitmapData, Brush brush, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, points);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPolygon(ctx, bitmapData, brush, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case FillPath with caching may perform better.
        // - When filling, bounds right/bottom are exclusive, so zero width or height means no operation.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => FillRectangle(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => FillRectangle(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA fill, if rectangle is integer
            if (
                (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.DoOperationSynchronously(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.DoOperationSynchronously(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return DirectDrawer.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rect, color);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return solidBrush.FillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, rect);
            }

            return DoFillRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

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

        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.BeginOperation(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        public static IAsyncResult BeginFillRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.BeginOperation(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions), asyncConfig);
        }

        public static bool EndFillRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillRectangle));

        #endregion

        #region Async TAP
#if !NET35

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

        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null })
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.DoOperationAsync(ctx => DirectDrawer.FillRectangle(ctx, bitmapData, rect, color), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, new SolidBrush(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        public static Task<bool> FillRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);

            // Shortcut for non-blended, non-AA thin lines
            if (brush is SolidBrush solidBrush
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, Quantizer: null, Ditherer: null }))
            {
                Rectangle rect = Rectangle.Truncate(rectangle);
                if (rect == rectangle)
                    return AsyncHelper.DoOperationAsync(ctx => solidBrush.FillRectangle(ctx, bitmapData, rect), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Ellipse
        // NOTE: Unlike the Rectangle methods, this section have no shortcuts because it wouldn't produce the same result as the Path-based fill,
        // so most of the overloads are just for symmetry reasons.

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        // Remarks:
        // - Fill overloads are here for symmetry with the Rectangle methods but there are no actual shortcuts for other filled shapes than rectangles.
        //   If you fill the same shape with any brush repeatedly, use FillPath with caching for the best performance.
        // - When filling, bounds right/bottom are exclusive, so zero width or height means no operation.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => FillEllipse(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillEllipse(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => FillEllipse(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillEllipse(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillEllipse(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions), asyncConfig);
        }

        public static bool EndFillEllipse(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillEllipse));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, new SolidBrush(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillEllipseAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillEllipse(ctx, bitmapData, brush, bounds, drawingOptions), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Pie

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => FillPie(bitmapData, color, new Rectangle(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillPie(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => FillPie(bitmapData, color, new RectangleF(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillPie(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillPie(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPie(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndFillPie(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPie));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, new SolidBrush(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillPieAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPie(ctx, bitmapData, brush, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case DrawPath with caching may perform better.
        // - When drawing, bounds right/bottom are inclusive, so zero width or height means 1 pixel wide/high bounds.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, int cornerRadius, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), cornerRadius, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float cornerRadius, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), cornerRadius, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => FillRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            DoFillRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool FillRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush);
            return DoFillRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

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

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillRoundedRectangle(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.BeginOperation(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndFillRoundedRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillRoundedRectangle));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, new SolidBrush(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> FillRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Brush brush, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush);
            return AsyncHelper.DoOperationAsync(ctx => DoFillRoundedRectangle(ctx, bitmapData, brush, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        // Remarks:
        // - If drawingOptions.Transformation is not the identity matrix, then the path region is not cached. To improve the performance of repeatedly drawn transformed paths, apply the transformations on the Path instance instead, and use the identity matrix in options.
        public static void FillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            DoFillPath(AsyncHelper.DefaultContext, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default);
        }

        public static bool FillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        public static bool FillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        public static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            return DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default);
        }

        public static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default);
        }

        public static IAsyncResult BeginFillPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.BeginOperation(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginFillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.BeginOperation(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndFillPath(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPath));

#if !NET35
        public static Task<bool> FillPathAsync(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPath(ctx, bitmapData, path, new SolidBrush(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

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

        private static void ValidateArguments(IWritableBitmapData bitmapData, Brush brush)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, Brush brush, IEnumerable points)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, Brush brush, Path path)
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
