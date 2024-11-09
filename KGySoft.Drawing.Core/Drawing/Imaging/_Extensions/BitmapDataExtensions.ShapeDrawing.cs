#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ShapeDrawing.cs
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
using System.Drawing;
using System.Runtime.CompilerServices;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, int x1, int y1, int x2, int y2, DrawingOptions? drawingOptions = null)
            => DrawLine(bitmapData, color, new Point(x1, y1), new Point(x2, y2), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color);
                return;
            }

            DoDrawLine(AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, float x1, float y1, float x2, float y2, DrawingOptions? drawingOptions = null)
            => DrawLine(bitmapData, color, new PointF(x1, y1), new PointF(x2, y2), drawingOptions);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions?.PixelOffset ?? 0f);
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

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions, ParallelConfig? parallelConfig)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions?.PixelOffset ?? 0f);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, parallelConfig);
            }

            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2, drawingOptions?.PixelOffset ?? 0f);
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

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA thin lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions?.PixelOffset ?? 0f);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2);
                return context?.IsCancellationRequested != true;
            }

            return DoDrawLine(context ?? AsyncHelper.DefaultContext, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool DrawLine(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2, drawingOptions?.PixelOffset ?? 0f);
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

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions?.PixelOffset ?? 0f);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.BeginOperation(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static IAsyncResult BeginDrawLine(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2, drawingOptions?.PixelOffset ?? 0f);
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

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Color32 color, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData);

            // Shortcut for solid non-AA lines
            if (color.A == Byte.MaxValue && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                || color.A != Byte.MaxValue && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null })
            {
                ThinPathDrawer.DrawLine(bitmapData, p1, p2, color, drawingOptions?.PixelOffset ?? 0f);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, new Pen(color), p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static Task<bool> DrawLineAsync(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen);

            // Shortcut for solid non-AA thin lines
            if (pen is { Brush: SolidBrush solidBrush, Width: <= 1f and >= 0.25f }
                && (!solidBrush.HasAlpha && drawingOptions is null or { AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }
                    || solidBrush.HasAlpha && drawingOptions is { AlphaBlending: false, AntiAliasing: false, IsIdentityTransform: true, FastThinLines: true, Quantizer: null, Ditherer: null }))
            {
                solidBrush.DrawThinLineDirect(bitmapData, p1, p2, drawingOptions?.PixelOffset ?? 0f);
                return AsyncHelper.FromResult(true, asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoDrawLine(ctx, bitmapData, pen, p1, p2, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

#endif
        #endregion

        #endregion

        #region Path

        #region DrawPath

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        public static bool DrawPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Pen pen, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return DoDrawPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default);
        }

        public static IAsyncResult BeginDrawPath(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.BeginOperation(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndDrawPath(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginDrawPath));

#if !NET35
        public static Task<bool> DrawPathAsync(this IReadWriteBitmapData bitmapData, Pen pen, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, pen, path);
            return AsyncHelper.DoOperationAsync(ctx => DoDrawPath(ctx, bitmapData, path, pen, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }
#endif

        #endregion

        #region FillPath

        public static bool FillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, ParallelConfig? parallelConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.DoOperationSynchronously(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), parallelConfig);
        }

        // Remarks:
        // - If drawingOptions.Transformation is not the identity matrix, then the path region is not cached. To improve the performance of transformed paths, apply the transformations on the Path instance instead, and use the identity matrix in options.
        public static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Brush brush, Path path, DrawingOptions? drawingOptions = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default);
        }

        public static IAsyncResult BeginFillPath(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, AsyncConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.BeginOperation(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }

        public static bool EndFillPath(this IAsyncResult asyncResult) => AsyncHelper.EndOperation<bool>(asyncResult, nameof(BeginFillPath));

#if !NET35
        public static Task<bool> FillPathAsync(this IReadWriteBitmapData bitmapData, Brush brush, Path path, DrawingOptions? drawingOptions = null, TaskConfig? asyncConfig = null)
        {
            ValidateArguments(bitmapData, brush, path);
            return AsyncHelper.DoOperationAsync(ctx => DoFillPath(ctx, bitmapData, path, brush, drawingOptions ?? DrawingOptions.Default), asyncConfig);
        }
#endif

        #endregion

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

        private static void ValidateArguments(IWritableBitmapData bitmapData, Pen pen, Path path)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);
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

        #region Line

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoDrawLine(IAsyncContext context, IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions drawingOptions)
            => DoDrawPath(context, bitmapData, new Path(false).AddLine(p1, p2), pen, drawingOptions);

        #endregion

        #region Path

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
