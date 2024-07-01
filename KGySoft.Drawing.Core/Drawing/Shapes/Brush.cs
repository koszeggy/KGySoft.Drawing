#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Brush.cs
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

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a brush shape filling operations.
    /// </summary>
    internal abstract class Brush // TODO: public when it can fill any path, IEquatable, ICloneable
    {
        #region Methods

        internal void ApplyPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions)
        {

            RawPath rawPath = path/*TODO .AsClosed() - if needed, cache it, too */.RawPath;
            // TODO: if rawPath.TryGetRegion(this, bitmapData, drawingOptions, out region) -> ApplyRegion... - important: if [Row]Path is not disposable, cached regions should not use ArrayPool or unmanaged buffers
            Rectangle bounds = Rectangle.Intersect(rawPath.Bounds, new Rectangle(Point.Empty, bitmapData.Size));

            if (bounds.IsEmpty)
                return;

            using FillPathSession session = CreateSession(bitmapData, bounds, drawingOptions);
            RegionScanner scanner = GetScanner(/*closed*/path, bounds, drawingOptions);
            Array scanlineMask = session.GetMask(); // byte/ushort/float
            bool isMaskDirty = false;

            // TODO: parallel
            while (scanner.MoveNextRow())
            {
                if (isMaskDirty)
                    Array.Clear(scanlineMask);

                isMaskDirty = scanner.ScanCurrentRow(scanlineMask);
                if (isMaskDirty)
                    session.ApplyRow(scanlineMask);
            }
        }

        internal abstract void ApplyRegion(IAsyncContext context, IReadWriteBitmapData bitmapData, IReadableBitmapData region, Path path, DrawingOptions drawingOptions);

        #endregion
    }
}