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

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

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
                    DirectDrawer.FillRectangle(AsyncHelper.DefaultContext, bitmapData, rect, color);
                return;
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
                return AsyncHelper.DoOperationSynchronously(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rectangle), parallelConfig);
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
                    return AsyncHelper.DoOperationSynchronously(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rect), parallelConfig);
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
                return solidBrush.FillRectangleDirect(context ?? AsyncHelper.DefaultContext, bitmapData, rectangle);
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
                    return solidBrush.FillRectangleDirect(context ?? AsyncHelper.DefaultContext, bitmapData, rect);
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
                return AsyncHelper.BeginOperation(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rectangle), asyncConfig);
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
                    return AsyncHelper.BeginOperation(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rect), asyncConfig);
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
                return AsyncHelper.DoOperationAsync(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rectangle), asyncConfig);
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
                    return AsyncHelper.DoOperationAsync(ctx => solidBrush.FillRectangleDirect(ctx, bitmapData, rect), asyncConfig);
            }

            return AsyncHelper.DoOperationAsync(ctx => DoFillRectangle(ctx, bitmapData, brush, rectangle, drawingOptions), asyncConfig);
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

        // TODO: remove? Now it's for symmetry, but at DrawLine(s)/etc. it's justified because it has a special handling for thin lines, but here this is just a shortcut with new Pen(color)
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

        #region Rectangle

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static bool DoFillRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Brush brush, RectangleF rectangle, DrawingOptions drawingOptions)
            => DoFillPath(context, bitmapData, new Path(false).AddRectangle(rectangle), brush, drawingOptions);

        #endregion

        #region Path

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
