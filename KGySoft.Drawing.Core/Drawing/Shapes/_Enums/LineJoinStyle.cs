#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LineJoinStyle.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    /// Represents the possible styles for joining two lines.
    /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
    /// </summary>
    public enum LineJoinStyle
    {
        /// <summary>
        /// Represents a miter join. This produces a sharp corner or a bevel join if the miter limit is exceeded.
        /// </summary>
        Miter,

        /// <summary>
        /// Represents a bevel join, which produces a diagonal corner.
        /// </summary>
        Bevel,

        /// <summary>
        /// Represents a round join, which produces a rounded corner.
        /// </summary>
        Round
    }
}