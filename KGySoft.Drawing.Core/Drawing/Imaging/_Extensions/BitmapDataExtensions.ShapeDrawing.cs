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

using System.Drawing;

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
            DoDrawPath(AsyncHelper.DefaultContext, bitmapData, new Path().AddLine(p1, p2), pen, drawingOptions);
        }

        #endregion

        #region Path

        internal static bool DrawPath(this IReadWriteBitmapData bitmapData, IAsyncContext? context, Path path, Pen pen, DrawingOptions? drawingOptions = null)
        {

            // TODO: make public when path contains every possible shapes

            //ValidateArguments(...);

            // TODO: fast shortcut when possible (solid non-AA 1px pen)

            DoDrawPath(context ?? AsyncHelper.DefaultContext, bitmapData, path, pen, drawingOptions);
            return context?.IsCancellationRequested != true;
        }

        #endregion

        #endregion

        #region Private Methods

        private static void DoDrawPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, Pen pen, DrawingOptions? drawingOptions)
        {
            drawingOptions ??= DrawingOptions.Default;
            IReadableBitmapData? region = path.GetRegion(context, pen, drawingOptions);
            if (region != null)
                pen.Brush.ApplyRegion(context, bitmapData, region, path, drawingOptions);
        }

        #endregion

        #endregion
    }
}
