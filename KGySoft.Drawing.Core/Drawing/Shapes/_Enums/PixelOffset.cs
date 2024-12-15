#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelOffset.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents pixel offset strategies for the see <see cref="DrawingOptions.ScanPathPixelOffset">DrawingOptions.ScanPathPixelOffset</see>
    /// and <see cref="DrawingOptions.DrawPathPixelOffset">DrawingOptions.DrawPathPixelOffset</see> properties.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="DrawingOptions.ScanPathPixelOffset">DrawingOptions.ScanPathPixelOffset</see>
    /// and <see cref="DrawingOptions.DrawPathPixelOffset">DrawingOptions.DrawPathPixelOffset</see> properties for details and image examples.
    /// </summary>
    public enum PixelOffset
    {
        /// <summary>
        /// When scanning the region of a path (see <see cref="DrawingOptions.ScanPathPixelOffset">DrawingOptions.ScanPathPixelOffset</see>), it specifies that the scanning of edges
        /// should be performed at the top of the pixels. When drawing a path (see <see cref="DrawingOptions.DrawPathPixelOffset">DrawingOptions.DrawPathPixelOffset</see>),
        /// it specifies that the points of the drawn path are not adjusted before applying the pen width.
        /// </summary>
        None,

        /// <summary>
        /// When scanning the region of a path (see <see cref="DrawingOptions.ScanPathPixelOffset">DrawingOptions.ScanPathPixelOffset</see>), it specifies that the scanning of edges
        /// should be performed at the center of the pixels. When drawing a path (see <see cref="DrawingOptions.DrawPathPixelOffset">DrawingOptions.DrawPathPixelOffset</see>),
        /// it specifies that the points of the drawn path are shifted by a half pixel before applying the pen width.
        /// </summary>
        Half,
    }
}