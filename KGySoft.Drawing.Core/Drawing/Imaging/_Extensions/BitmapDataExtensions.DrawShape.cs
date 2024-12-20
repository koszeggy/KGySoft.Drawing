﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.DrawShape.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

#endregion

#region Used Aliases

#if NETFRAMEWORK
using Pen = KGySoft.Drawing.Shapes.Pen;
using SolidBrush = KGySoft.Drawing.Shapes.SolidBrush;
#endif

#endregion

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

        #region Line

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case DrawPath with caching may perform better.
        // - Possible OverflowException when a Path is created internally and bounds.With overflows.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, int x1, int y1, int x2, int y2, DrawingOptions? drawingOptions = null)
            => DrawLine(bitmapData, color, new Point(x1, y1), new Point(x2, y2), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color);
                return;
            }

            DoDrawLine(AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        // Remarks:
        // - Possible OverflowException
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, float x1, float y1, float x2, float y2, DrawingOptions? drawingOptions = null)
            => DrawLine(bitmapData, color, new PointF(x1, y1), new PointF(x2, y2), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawLine(AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawLine(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawLine));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLine(bitmapData, p1, p2, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Lines

        #region Sync

        #region Default Context

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color);
                return;
            }

            DoDrawLines(AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawLines(AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        // Into remarks: when shortcutting, the operation cannot be cancelled, there is no report progress, and we always return true. To avoid that, use DrawPath instead.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLines(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLines(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLines(context ?? AsyncHelper.DefaultContext, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLines(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLines(context ?? AsyncHelper.DefaultContext, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLines(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLines(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLines(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawLines(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawLines));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawLinesAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLinesAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawLines(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLines(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLinesAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLinesAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawLines(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLines(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Beziers

        #region Sync

        #region Default Context

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawBeziers(AsyncHelper.DefaultContext, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawBeziers(AsyncHelper.DefaultContext, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        // Into remarks: when shortcutting, the operation cannot be cancelled, there is no report progress, and we always return true. To avoid that, use DrawPath instead.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawBeziers(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawBeziers(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawBeziers(context ?? AsyncHelper.DefaultContext, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawBeziers(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawBeziers(context ?? AsyncHelper.DefaultContext, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawBeziers(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawBeziers(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawBeziers(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawBeziers(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawBeziers));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawBeziersAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawBeziersAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawBeziers(bitmapData, pointsList, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawBeziers(ctx, bitmapData, new Pen(color), pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawBeziersAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawBeziersAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points, out List<PointF> pointsList);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawBeziers(bitmapData, pointsList, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawBeziers(ctx, bitmapData, pen, pointsList, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Polygon

        #region Sync

        #region Default Context

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color);
                return;
            }

            DoDrawPolygon(AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawPolygon(AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        // Into remarks: when shortcutting, the operation cannot be cancelled, there is no report progress, and we always return true. To avoid that, use DrawPath instead.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPolygon(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPolygon(context ?? AsyncHelper.DefaultContext, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPolygon(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPolygon(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPolygon(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawPolygon(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawPolygon));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPolygonAsync(this IReadWriteBitmapData bitmapData, Color32 color, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, points);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPolygon(bitmapData, points, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPolygon(ctx, bitmapData, new Pen(color), points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPolygonAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPolygonAsync(this IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, points);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPolygon(bitmapData, points, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPolygon(ctx, bitmapData, pen, points, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Rectangle

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case DrawPath with caching may perform better.
        // - When drawing, bounds right/bottom are inclusive, so zero width or height means 1 pixel wide/high rectangle.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => DrawRectangle(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color);
                return;
            }

            DoDrawRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => DrawRectangle(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle rectangle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF rectangle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawRectangle));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRectangle(bitmapData, rectangle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRectangle(ctx, bitmapData, new Pen(color), rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRectangle(bitmapData, rectangle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRectangle(ctx, bitmapData, pen, rectangle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Ellipse

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case DrawPath with caching may perform better.
        // - When drawing, bounds right/bottom are inclusive, so zero width or height means 1 pixel wide/high bounds.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, DrawingOptions? drawingOptions = null)
            => DrawEllipse(bitmapData, color, new Rectangle(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color);
                return;
            }

            DoDrawEllipse(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, DrawingOptions? drawingOptions = null)
            => DrawEllipse(bitmapData, color, new RectangleF(x, y, width, height), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawEllipse(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawEllipse(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF bounds, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawEllipse(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawEllipse(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawEllipse(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawEllipse(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawEllipse(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawEllipse));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawEllipseAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawEllipse(bitmapData, bounds, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawEllipse(ctx, bitmapData, new Pen(color), bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawEllipseAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawEllipseAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawEllipse(bitmapData, bounds, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawEllipse(ctx, bitmapData, pen, bounds, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Arc

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        // Remarks:
        // - When cannot do a shortcut, a Path is created internally. In such case DrawPath with caching may perform better.
        // - When drawing, bounds right/bottom are inclusive, so zero width or height means 1 pixel wide/high bounds.
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => DrawArc(bitmapData, color, new Rectangle(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color);
                return;
            }

            DoDrawArc(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => DrawArc(bitmapData, color, new RectangleF(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawArc(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawArc(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawArc(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawArc(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawArc(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawArc(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawArc(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawArc(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawArc(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawArc(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawArc(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawArc));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawArcAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawArcAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawArc(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawArc(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawArcAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawArcAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawArc(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawArc(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Pie

        #region Sync

        #region Default Context
        // NOTE: Only this section has separate int/float overloads for convenience reasons.

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => DrawPie(bitmapData, color, new Rectangle(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawPie(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
            => DrawPie(bitmapData, color, new RectangleF(x, y, width, height), startAngle, sweepAngle, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawPie(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPie(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPie(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPie(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawPie(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawPie(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawPie(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPie(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPie(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPie(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawPie(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawPie));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPieAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawPie(bitmapData, bounds, startAngle, sweepAngle, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPie(ctx, bitmapData, new Pen(color), bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPieAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPieAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float startAngle, float sweepAngle, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawPie(bitmapData, bounds, startAngle, sweepAngle, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawPie(ctx, bitmapData, pen, bounds, startAngle, sweepAngle, drawingOptions ?? DrawingOptions.Default), asyncConfig);
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
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height, int cornerRadius, DrawingOptions? drawingOptions = null)
            => DrawRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), cornerRadius, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, int x, int y, int width, int height,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => DrawRoundedRectangle(bitmapData, color, new Rectangle(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color);
                return;
            }

            DoDrawRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height, float cornerRadius, DrawingOptions? drawingOptions = null)
            => DrawRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), cornerRadius, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, float x, float y, float width, float height,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
            => DrawRoundedRectangle(bitmapData, color, new RectangleF(x, y, width, height), radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return;
            }

            DoDrawRoundedRectangle(AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #region ParallelConfig
        // NOTE: These overloads could be combined with the default context ones, but we keep them separated for performance reasons (see DrawLineShortcutTest in performance tests).

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        #endregion

        #region IAsyncContext

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawRoundedRectangle(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return context?.IsCancellationRequested != true;
            }

            return DoDrawRoundedRectangle(context ?? AsyncHelper.DefaultContext, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default);
        }

        #endregion

        #endregion

        #region Async APM

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, 
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawRoundedRectangle(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawRoundedRectangle(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawRoundedRectangle));

        #endregion

        #region Async TAP
#if !NET35

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Color32 color, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for non-blended, non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                DirectDrawer.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, color, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, new Pen(color), bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds, int cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, Rectangle bounds,
            int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds, float cornerRadius, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, cornerRadius, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, cornerRadius, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawRoundedRectangleAsync(this IReadWriteBitmapData bitmapData, Pen pen, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for non-blended, non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawRoundedRectangle(bitmapData, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions.PixelOffset());
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawRoundedRectangle(ctx, bitmapData, pen, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Path

        // Remarks:
        // - If drawingOptions.Transformation is not the identity matrix, then the path region is not cached. To improve the performance of repeatedly drawn transformed paths, apply the transformations on the Path instance instead, and use the identity matrix in options.
        public static void DrawPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            DoDrawPath(AsyncHelper.DefaultContext, bitmapData, path, new Pen(color), drawingOptions ?? DrawingOptions.Default);
        }

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPath(ctx, bitmapData, path, new Pen(color), drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, path);
            return DoDrawPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, new Pen(color), drawingOptions ?? DrawingOptions.Default);
        }

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return DoDrawPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default);
        }

        public static IAsyncResult BeginDrawPath(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.BeginOperation(ctx => DoDrawPath(ctx, bitmapData, path, new Pen(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawPath(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.BeginOperation(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawPath(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawPath));

#if !NET35
        public static Task<bool> DrawPathAsync(this IReadWriteBitmapData bitmapData, Color32 color, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, path);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawPath(ctx, bitmapData, path, new Pen(color), drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawPathAsync(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        private static void ValidateArguments(IWritableBitmapData bitmapData, Pen pen)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, IEnumerable points)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Validation")]
        private static void ValidateArguments(IWritableBitmapData bitmapData, IEnumerable points, out List<PointF> pointsList)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            pointsList = new List<PointF>(points switch
            {
                IEnumerable<Point> pointsI32 => pointsI32.Select(p => (PointF)p),
                IEnumerable<PointF> pointsF => pointsF,
                _ => throw new InvalidOperationException(Res.InternalError("Unexpected points type"))
            });

            if (pointsList.Count != 0 && (pointsList.Count - 1) % 3 != 0)
                throw new ArgumentException(nameof(points), Res.ShapesBezierPointsInvalid);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, Pen pen, IEnumerable points)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "Validation")]
        private static void ValidateArguments(IWritableBitmapData bitmapData, Pen pen, IEnumerable points, out List<PointF> pointsList)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
            if (points == null)
                throw new ArgumentNullException(nameof(points), PublicResources.ArgumentNull);
            pointsList = new List<PointF>(points switch
            {
                IEnumerable<Point> pointsI32 => pointsI32.Select(p => (PointF)p),
                IEnumerable<PointF> pointsF => pointsF,
                _ => throw new InvalidOperationException(Res.InternalError("Unexpected points type"))
            });

            if (pointsList.Count != 0 && (pointsList.Count - 1) % 3 != 0)
                throw new ArgumentException(nameof(points), Res.ShapesBezierPointsInvalid);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, Path path)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
        }

        private static void ValidateArguments(IWritableBitmapData bitmapData, Pen pen, Path path)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
        }

        #endregion

        #region DoDrawXXX

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawLine(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddLine(p1, p2), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawLines(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions drawingOptions)
            => DoDrawLines(context, bitmapData, pen, points.Select(p => (PointF)p), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawLines(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddLines(points), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawBeziers(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, List<PointF> points, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddBeziers(points), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawPolygon(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<Point> points, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddPolygon(points.Select(p => (PointF)p)), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawPolygon(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, IEnumerable<PointF> points, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddPolygon(points), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddRectangle(rectangle), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawEllipse(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddEllipse(rectangle), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawArc(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, float startAngle, float sweepAngle, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddArc(rectangle, startAngle, sweepAngle), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawPie(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, float startAngle, float sweepAngle, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddPie(rectangle, startAngle, sweepAngle), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawRoundedRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle, float cornerRadius, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddRoundedRectangle(rectangle, cornerRadius), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawRoundedRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, RectangleF rectangle,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddRoundedRectangle(rectangle, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft), pen, drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, Pen pen, DrawingOptions drawingOptions)
        {
            if (!drawingOptions.IsIdentityTransform)
            {
                path = Path.Transform(path, drawingOptions.Transformation);
                path.PreferCaching = false;
            }

            return pen.DrawPath(context, bitmapData, path, drawingOptions);
        }

        #endregion

        #endregion

        #endregion
    }
}
