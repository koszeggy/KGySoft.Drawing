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

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    #region Usings

    // Should be inside namespace so Pen is not ambiguous between System.Drawing and KGySoft.Drawing.Shapes
    using KGySoft.Drawing.Shapes;

    #endregion
    
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        #region Line

        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, int x1, int y1, int x2, int y2, DrawingOptions? drawingOptions = null)
        {
            // TODO: fast shortcut
            DrawLine(bitmapData, new Pen(color), new PointF(x1, y1), new PointF(x2, y2), drawingOptions);
        }

        public static void DrawLine(this IReadWriteBitmapData bitmapData, Color32 color, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            // TODO: fast shortcut
            DrawLine(bitmapData, new Pen(color), (PointF)p1, p2, drawingOptions);
        }

        internal static void DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, int x1, int y1, int x2, int y2, DrawingOptions? drawingOptions = null)
        {
            // TODO: fast shortcut when possible (solid non-AA 1px pen)
            DrawLine(bitmapData, pen, new PointF(x1, y1), new PointF(x2, y2), drawingOptions);
        }

        internal static void DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, Point p1, Point p2, DrawingOptions? drawingOptions = null)
        {
            // TODO: fast shortcut when possible (solid non-AA 1px pen)
            DrawLine(bitmapData, pen, (PointF)p1, p2, drawingOptions);
        }

        internal static void DrawLine(this IReadWriteBitmapData bitmapData, Pen pen, PointF p1, PointF p2, DrawingOptions? drawingOptions = null)
        {
            // TODO: fast shortcut when possible (solid non-AA 1px pen)
            DoDrawPath(AsyncHelper.DefaultContext, bitmapData, new Path().AddLine(p1, p2), pen, drawingOptions, false);
        }

        #endregion

        #region Path

        internal static bool DrawPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Path path, Pen pen, DrawingOptions? drawingOptions = null, bool allowCachingPathRegion = true)
        {

            // TODO: make public when path contains every possible shapes

            //ValidateArguments(...);

            drawingOptions ??= DrawingOptions.Default;
            if (!drawingOptions.IsIdentityTransform)
            {
                path = Path.Transform(path, drawingOptions.Transformation);
                allowCachingPathRegion = false;
            }

            // TODO: fast shortcut when possible (solid non-AA 1px pen)

            DoDrawPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, pen, drawingOptions, allowCachingPathRegion);
            return context?.IsCancellationRequested != true;
        }

        // allowCachingPathRegion: true to allow caching the region of the specified path, so the next fill operation with the same, unchanged path will be faster; otherwise, false.
        // Remarks:
        // - Set allowCachingPathRegion to false if path instance is not stored or is only used once. - TODO: put unto options instead. Default: true, so the callers from a non-ath overload should replace null with a nondefault instance
        // - If drawingOptions.Transformation is not the identity matrix, then the path region is not cached. To improve the performance of transformed paths, apply the transformations on the Path instance instead, and use the identity matrix in options.
        internal static bool FillPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Path path, Brush brush, DrawingOptions? drawingOptions = null, bool allowCachingPathRegion = true)
        {
            // TODO: make public when path contains every possible shapes

            //ValidateArguments(...);

            drawingOptions ??= DrawingOptions.Default;
            if (!drawingOptions.IsIdentityTransform)
            {
                path = Path.Transform(path, drawingOptions.Transformation);
                allowCachingPathRegion = false;
            }

            // TODO: fast shortcut when possible (solid non-AA brush with no quantizer)

            DoFillPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, brush, drawingOptions, allowCachingPathRegion);
            return context?.IsCancellationRequested != true;
        }

        #endregion

        #endregion

        #region Private Methods

        private static void DoDrawPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, Pen pen, DrawingOptions drawingOptions, bool cache)
        {
            // TODO: put allow caching in options
            pen.ApplyPath(context, bitmapData, path, drawingOptions, cache);
        }

        private static void DoFillPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, Brush brush, DrawingOptions drawingOptions, bool cache)
        {
            // TODO: put allow caching in options
            brush.ApplyPath(context, bitmapData, path, drawingOptions, cache);
        }

        #endregion

        #endregion
    }
}
