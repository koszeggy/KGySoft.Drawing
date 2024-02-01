#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifCompressionMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the compression behavior of the <see cref="GifEncoder"/> class.
    /// </summary>
    public enum GifCompressionMode
    {
        /// <summary>
        /// Represents the automatic adaptive mode.
        /// The internally used code table might be regularly cleared.
        /// </summary>
        Auto,

        /// <summary>
        /// Indicates that once the internally used code table is full, it is not maintained anymore and all remaining codes
        /// are written using 12 bit code size. If codes so far can be re-used, then the result can be more compact than with <see cref="Auto"/> mode;
        /// otherwise, the result can be even longer than in <see cref="Uncompressed"/> mode.
        /// This option might not be tolerated by some decoders.
        /// </summary>
        DoNotClear,

        /// <summary>
        /// Indicates that all written codes must use the same bit size (3 to 9 bits, depending on palette size).
        /// It uses less memory but it may lead to poor compression, especially with palettes using no more than 4 colors.
        /// </summary>
        DoNotIncreaseBitSize,

        /// <summary>
        /// Indicates that the <see cref="GifEncoder"/> should not use any compression.
        /// It uses the least memory but the result can be really long.
        /// </summary>
        Uncompressed
    }
}