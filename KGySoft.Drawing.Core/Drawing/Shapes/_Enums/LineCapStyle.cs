#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LineCapStyle.cs
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
    /// Represents the possible styles for the ends of open figures.
    /// <br/>See the <strong>Examples</strong> section of the <see cref="Pen"/> class for image examples.
    /// </summary>
    public enum LineCapStyle
    {
        /// <summary>
        /// Represents a flat line cap. This produces a square line cap that does not extend past the end of the line.
        /// </summary>
        Flat,

        /// <summary>
        /// Represents a square line cap. The only difference from <see cref="Flat"/> is that the <see cref="Flat"/> style
        /// has the originally specified length, whereas the <see cref="LineCapStyle.Square"/> style extends the line by half of the <see cref="Pen"/> width.
        /// </summary>
        Square,

        /// <summary>
        /// Represents a round line cap.
        /// </summary>
        Round,

        /// <summary>
        /// Represents a triangle line cap.
        /// </summary>
        Triangle,
    }
}